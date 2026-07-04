namespace RKWorkspace.Shell.SpatialTray;

public sealed class SpatialTraySession
{
    private const string RoomId = "rkws-spatial-room-local";
    private const string ThingId = "thing-rechnung";
    public const string DefaultAblageId = "tablet";
    public const string DefaultTargetAblageId = "monitor";
    private readonly object _sync = new();
    private readonly SpatialTrayConfiguration _configuration;
    private SpatialRoomState _room;

    public SpatialTraySession(SpatialTrayConfiguration configuration)
    {
        _configuration = configuration.Validate();
        var now = DateTimeOffset.UtcNow;
        _room = new SpatialRoomState
        {
            RoomId = RoomId,
            Version = "1",
            Ablagen = CreateAblagen(now),
            Things =
            [
                new SpatialThing
                {
                    ThingId = ThingId,
                    DisplayName = _configuration.ThingName,
                    Kind = "Dokument",
                    CurrentState = SpatialThingState.RestingOnAblage,
                    CurrentAblageId = DefaultAblageId,
                    PositionOnAblage = new SpatialPoint(0.5, 0.46),
                    Metadata = new Dictionary<string, string>
                    {
                        ["humanRole"] = "work-object",
                        ["sourceExperience"] = "HX-001"
                    },
                    UpdatedAt = now
                }
            ],
            UpdatedAt = now
        };
    }

    public SpatialTrayState CurrentState
    {
        get
        {
            lock (_sync)
            {
                var thing = MainThing();
                return thing.CurrentState switch
                {
                    SpatialThingState.Picked => SpatialTrayState.ThingPicked,
                    SpatialThingState.Carried => SpatialTrayState.ThingPicked,
                    SpatialThingState.ApproachingAblage => SpatialTrayState.NearAblage,
                    SpatialThingState.PreviewOnAblage => SpatialTrayState.NearAblage,
                    SpatialThingState.PlacedOnAblage => SpatialTrayState.Placed,
                    SpatialThingState.FreePlaced => SpatialTrayState.Placed,
                    SpatialThingState.Cancelled => SpatialTrayState.Cancelled,
                    SpatialThingState.Lost => SpatialTrayState.Failed,
                    _ => SpatialTrayState.ThingOnTray
                };
            }
        }
    }

    public bool DesktopAblageHasThing
    {
        get
        {
            lock (_sync)
            {
                var thing = MainThing();
                return thing.CurrentAblageId == "monitor" &&
                    thing.CurrentState is SpatialThingState.PlacedOnAblage or SpatialThingState.RestingOnAblage;
            }
        }
    }

    public object Snapshot(string? surfaceAblageId = null)
    {
        lock (_sync)
        {
            var viewer = FindAblageOrDefault(surfaceAblageId ?? DefaultAblageId);
            var thing = MainThing();
            var surfaceThing = CreateSurfaceThingSnapshot(viewer.AblageId, thing);
            var bubblesVisible = ShouldShowBubbles(thing);
            var bubbles = bubblesVisible ? CreateBubbles(viewer.AblageId).ToArray() : Array.Empty<object>();
            return new
            {
                roomId = _room.RoomId,
                version = _room.Version,
                updatedAt = _room.UpdatedAt,
                state = CurrentState.ToString(),
                carryState = CarryStateText(),
                surface = CreateAblageSnapshot(viewer, SpatialAblageBubbleState.ReadableBubble, viewer.Distance, true),
                ablagen = _room.Ablagen.Select(ablage => CreateAblageSnapshot(ablage, SpatialAblageBubbleState.DistantBubble, ablage.Distance, true)).ToArray(),
                things = _room.Things.Select(CreateThingSnapshot).ToArray(),
                thing = "Digitales Ding",
                name = thing.DisplayName,
                activeCarry = _room.ActiveCarry is null ? null : CreateCarrySnapshot(_room.ActiveCarry),
                portalTransition = _room.ActivePortalTransition is null ? null : CreatePortalTransitionSnapshot(_room.ActivePortalTransition),
                activeAblage = viewer.DisplayName,
                activeAblageId = viewer.AblageId,
                bubblesVisible,
                compass = bubbles,
                bubbles,
                surfaceThing,
                motion = new
                {
                    nameRevealThreshold = _configuration.NameRevealThreshold,
                    activationThreshold = _configuration.ActivationThreshold,
                    bubbleScaleFactor = _configuration.BubbleScaleFactor,
                    microTextOpacity = _configuration.MicroTextOpacity,
                    wobbleAmplitude = _configuration.WobbleAmplitude,
                    wobbleFrequency = _configuration.WobbleFrequency,
                    softSnapStrength = _configuration.SoftSnapStrength,
                    tiltSource = "movement-vector",
                    vectorTiltMaxDegrees = _configuration.VectorTiltMaxDegrees,
                    initialResistanceDistancePx = _configuration.InitialResistanceDistancePx,
                    heldCompactScale = _configuration.HeldCompactScale,
                    liftDepthPx = _configuration.LiftDepthPx,
                    glideIntoBubbleMs = _configuration.GlideIntoBubbleMs,
                    snapMode = "soft-invitation"
                },
                haptics = new
                {
                    mobilePrepared = _configuration.MobileHapticsPrepared,
                    opticalPrepared = _configuration.OpticalHapticsPrepared,
                    digitalHand = true,
                    partialOcclusion = true,
                    occlusionRatio = _configuration.HandOcclusionRatio,
                    contactShadow = true,
                    compactWhenHeld = true,
                    vibrateOnPickMs = 8,
                    vibrateOnLiftMs = 9,
                    vibrateOnAblageMs = 12,
                    vibrateOnPlaceMs = 16
                },
                transition = new
                {
                    bubbleOpens = true,
                    portalPhases = true,
                    portalTransition = true,
                    targetGhostBeforePlace = true,
                    glideIntoBubble = true,
                    objectEmerges = true,
                    placeAfterGlide = true,
                    targetPositioning = "relative-on-ablage",
                    enteringProgress = _configuration.PortalEnteringProgress,
                    emergingProgress = _configuration.PortalEmergingProgress,
                    carriedScale = _configuration.HeldCompactScale,
                    previewScale = 0.74,
                    placedScale = 1.0
                },
                placement = new
                {
                    kind = PlacementKind(thing),
                    x = thing.PositionOnAblage?.X,
                    y = thing.PositionOnAblage?.Y,
                    placedAt = thing.UpdatedAt
                },
                releaseMeansPlace = true,
                cancelReturnsToSource = true,
                status = StatusText(viewer.AblageId, thing),
                placedAt = thing.CurrentState is SpatialThingState.PlacedOnAblage or SpatialThingState.FreePlaced
                    ? thing.UpdatedAt
                    : (DateTimeOffset?)null
            };
        }
    }

    public void Pick()
    {
        Pick(DefaultAblageId);
    }

    public void Pick(string carrierAblageId)
    {
        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;
            var carrier = FindAblageOrDefault(carrierAblageId);
            var thing = MainThing();
            var sourceAblageId = thing.CurrentAblageId ?? carrier.AblageId;
            var carryId = $"carry-{Guid.NewGuid():N}";
            var carry = new SpatialCarrySession
            {
                CarryId = carryId,
                ThingId = thing.ThingId,
                SourceAblageId = sourceAblageId,
                CarrierAblageId = carrier.AblageId,
                State = SpatialCarrySessionState.Picked,
                StartedAt = now,
                UpdatedAt = now
            };
            ReplaceThing(thing with
            {
                CurrentState = SpatialThingState.Carried,
                CurrentAblageId = null,
                CurrentCarryId = carryId,
                PreviewAblageId = null,
                PositionOnAblage = null,
                UpdatedAt = now
            });
            _room = _room with { ActiveCarry = carry, ActivePortalTransition = null, UpdatedAt = now };
        }
    }

    public void Carry()
    {
        Move(DefaultAblageId, null, null);
    }

    public void Move(string carrierAblageId, double? x, double? y)
    {
        lock (_sync)
        {
            if (_room.ActiveCarry is null)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var carry = _room.ActiveCarry with
            {
                CarrierAblageId = FindAblageOrDefault(carrierAblageId).AblageId,
                State = SpatialCarrySessionState.Carried,
                Position = x.HasValue && y.HasValue ? new SpatialPoint(x.Value, y.Value) : _room.ActiveCarry.Position,
                UpdatedAt = now
            };
            ReplaceThing(MainThing() with
            {
                CurrentState = SpatialThingState.Carried,
                CurrentAblageId = null,
                CurrentCarryId = carry.CarryId,
                PreviewAblageId = null,
                PositionOnAblage = null,
                UpdatedAt = now
            });
            _room = _room with { ActiveCarry = carry, UpdatedAt = now };
        }
    }

    public void NearAblage(string ablage)
    {
        Approach(DefaultAblageId, ablage);
    }

    public void Approach(string carrierAblageId, string targetAblageId)
    {
        lock (_sync)
        {
            if (_room.ActiveCarry is null)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var carrier = FindAblageOrDefault(carrierAblageId);
            var target = FindAblageOrDefault(targetAblageId);
            var carry = _room.ActiveCarry with
            {
                CarrierAblageId = carrier.AblageId,
                TargetCandidateAblageId = target.AblageId,
                State = SpatialCarrySessionState.OpeningAblage,
                UpdatedAt = now
            };
            var transition = new SpatialPortalTransition
            {
                TransitionId = $"portal-{Guid.NewGuid():N}",
                ThingId = ThingId,
                SourceAblageId = carry.SourceAblageId,
                TargetAblageId = target.AblageId,
                State = SpatialPortalTransitionState.ReadyToPlace,
                Progress = _configuration.PortalEmergingProgress,
                SourceVisualProgress = _configuration.PortalEnteringProgress,
                TargetVisualProgress = _configuration.PortalEmergingProgress,
                StartedAt = now,
                UpdatedAt = now
            };
            ReplaceThing(MainThing() with
            {
                CurrentState = SpatialThingState.PreviewOnAblage,
                CurrentAblageId = null,
                CurrentCarryId = carry.CarryId,
                PreviewAblageId = target.AblageId,
                PositionOnAblage = null,
                UpdatedAt = now
            });
            _room = _room with { ActiveCarry = carry, ActivePortalTransition = transition, UpdatedAt = now };
        }
    }

    public void Release(double? x, double? y, string? ablage = null, bool placeOnAblage = false)
    {
        if (placeOnAblage && !string.IsNullOrWhiteSpace(ablage))
        {
            Place(ablage, x, y);
            return;
        }

        FreePlace(x, y);
    }

    public void FreePlace(double? x, double? y)
    {
        lock (_sync)
        {
            if (_room.ActiveCarry is null)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var carry = _room.ActiveCarry;
            ReplaceThing(MainThing() with
            {
                CurrentState = SpatialThingState.FreePlaced,
                CurrentAblageId = carry.CarrierAblageId,
                CurrentCarryId = null,
                PreviewAblageId = null,
                PositionOnAblage = PlacementPoint(x, y),
                UpdatedAt = now
            });
            _room = _room with
            {
                ActiveCarry = carry with { State = SpatialCarrySessionState.Placed, UpdatedAt = now },
                ActivePortalTransition = null,
                UpdatedAt = now
            };
        }
    }

    public void Place(string? ablage = null)
    {
        Place(ablage, null, null);
    }

    public void Place(string? ablage, double? x, double? y)
    {
        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;
            var target = FindAblageOrDefault(ablage ?? _room.ActiveCarry?.TargetCandidateAblageId ?? DefaultTargetAblageId);
            var carry = _room.ActiveCarry;
            ReplaceThing(MainThing() with
            {
                CurrentState = SpatialThingState.PlacedOnAblage,
                CurrentAblageId = target.AblageId,
                CurrentCarryId = null,
                PreviewAblageId = null,
                PositionOnAblage = PlacementPoint(x, y),
                UpdatedAt = now
            });
            _room = _room with
            {
                ActiveCarry = carry is null ? null : carry with
                {
                    TargetCandidateAblageId = target.AblageId,
                    State = SpatialCarrySessionState.Placed,
                    UpdatedAt = now
                },
                ActivePortalTransition = _room.ActivePortalTransition is null
                    ? null
                    : _room.ActivePortalTransition with
                    {
                        TargetAblageId = target.AblageId,
                        State = SpatialPortalTransitionState.Placed,
                        Progress = 1.0,
                        SourceVisualProgress = 1.0,
                        TargetVisualProgress = 1.0,
                        UpdatedAt = now
                    },
                UpdatedAt = now
            };
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            if (_room.ActiveCarry is null)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var carry = _room.ActiveCarry;
            ReplaceThing(MainThing() with
            {
                CurrentState = SpatialThingState.Cancelled,
                CurrentAblageId = carry.SourceAblageId,
                CurrentCarryId = null,
                PreviewAblageId = null,
                PositionOnAblage = new SpatialPoint(0.5, 0.46),
                UpdatedAt = now
            });
            _room = _room with
            {
                ActiveCarry = carry with { State = SpatialCarrySessionState.Cancelled, UpdatedAt = now },
                ActivePortalTransition = _room.ActivePortalTransition is null
                    ? null
                    : _room.ActivePortalTransition with
                    {
                        State = SpatialPortalTransitionState.Cancelled,
                        UpdatedAt = now
                    },
                UpdatedAt = now
            };
        }
    }

    private IReadOnlyList<SpatialAblage> CreateAblagen(DateTimeOffset now)
    {
        return
        [
            new SpatialAblage
            {
                AblageId = "handy",
                DisplayName = "Ablage Handy",
                SurfaceType = SpatialSurfaceType.Handheld,
                RelativePosition = "bei mir",
                Distance = SpatialAblageDistance.VeryNear,
                LastSeen = now,
                Position = new SpatialPoint(0.23, 0.68),
                Metadata = new Dictionary<string, string>
                {
                    ["shortName"] = "Handy",
                    ["roomRole"] = "near-surface"
                }
            },
            new SpatialAblage
            {
                AblageId = "monitor",
                DisplayName = _configuration.DesktopAblageName,
                SurfaceType = SpatialSurfaceType.Monitor,
                RelativePosition = "rechts",
                Distance = SpatialAblageDistance.Near,
                LastSeen = now,
                Position = new SpatialPoint(0.78, 0.42),
                Metadata = new Dictionary<string, string>
                {
                    ["shortName"] = "Monitor",
                    ["roomRole"] = "wide-surface"
                }
            },
            new SpatialAblage
            {
                AblageId = "tablet",
                DisplayName = "Ablage Tablet",
                SurfaceType = SpatialSurfaceType.Tablet,
                RelativePosition = "vorne",
                Distance = SpatialAblageDistance.Medium,
                LastSeen = now,
                Position = new SpatialPoint(0.52, 0.20),
                Metadata = new Dictionary<string, string>
                {
                    ["shortName"] = "Tablet",
                    ["roomRole"] = "mobile-surface"
                }
            },
            new SpatialAblage
            {
                AblageId = "desktop",
                DisplayName = "Ablage Desktop",
                SurfaceType = SpatialSurfaceType.Desktop,
                RelativePosition = "links",
                Distance = SpatialAblageDistance.Far,
                LastSeen = now,
                Position = new SpatialPoint(0.18, 0.44),
                Metadata = new Dictionary<string, string>
                {
                    ["shortName"] = "Tisch",
                    ["roomRole"] = "side-surface"
                }
            },
            new SpatialAblage
            {
                AblageId = "beamer",
                DisplayName = "Ablage Beamer",
                SurfaceType = SpatialSurfaceType.Projection,
                RelativePosition = "hinten",
                Distance = SpatialAblageDistance.VeryFar,
                LastSeen = now,
                Position = new SpatialPoint(0.55, 0.10),
                Metadata = new Dictionary<string, string>
                {
                    ["shortName"] = "Wand",
                    ["roomRole"] = "far-surface"
                }
            }
        ];
    }

    private IEnumerable<object> CreateBubbles(string viewerAblageId)
    {
        return _room.Ablagen
            .Where(ablage => !string.Equals(ablage.AblageId, viewerAblageId, StringComparison.Ordinal))
            .Select(ablage =>
            {
                var distance = DistanceFor(viewerAblageId, ablage.AblageId);
                var state = BubbleStateFor(ablage.AblageId, distance);
                return CreateAblageSnapshot(ablage, state, distance, false);
            });
    }

    private bool ShouldShowBubbles(SpatialThing thing)
    {
        if (_room.ActiveCarry?.State is not (
            SpatialCarrySessionState.Picked or
            SpatialCarrySessionState.Carried or
            SpatialCarrySessionState.NearAblage or
            SpatialCarrySessionState.OpeningAblage or
            SpatialCarrySessionState.PreviewingOnAblage))
        {
            return false;
        }

        return thing.CurrentState is
            SpatialThingState.Picked or
            SpatialThingState.Carried or
            SpatialThingState.ApproachingAblage or
            SpatialThingState.PreviewOnAblage;
    }

    private object CreateAblageSnapshot(
        SpatialAblage ablage,
        SpatialAblageBubbleState state,
        SpatialAblageDistance distance,
        bool forceReadable)
    {
        return new
        {
            id = ablage.AblageId,
            ablageId = ablage.AblageId,
            displayName = ablage.DisplayName,
            shortName = ablage.Metadata.TryGetValue("shortName", out var shortName) ? shortName : ablage.DisplayName,
            label = ablage.DisplayName,
            surfaceType = ablage.SurfaceType.ToString(),
            relativePosition = ablage.RelativePosition,
            direction = ablage.RelativePosition,
            distance = distance.ToString(),
            state = state.ToString(),
            portalPhase = state.ToString(),
            isAvailable = ablage.IsAvailable,
            isActive = state is SpatialAblageBubbleState.OpeningPortal or SpatialAblageBubbleState.PortalOpen or SpatialAblageBubbleState.ObjectEntering or SpatialAblageBubbleState.ObjectEmerging,
            opens = state is SpatialAblageBubbleState.OpeningPortal or SpatialAblageBubbleState.PortalOpen or SpatialAblageBubbleState.ObjectEntering or SpatialAblageBubbleState.ObjectEmerging,
            isPortal = state is SpatialAblageBubbleState.OpeningPortal or SpatialAblageBubbleState.PortalOpen or SpatialAblageBubbleState.ObjectEntering or SpatialAblageBubbleState.ObjectEmerging,
            canReceive = ablage.CanReceive,
            canProvide = ablage.CanProvide,
            metadata = ablage.Metadata,
            lastSeen = ablage.LastSeen,
            x = EdgePositionFor(ablage).X,
            y = EdgePositionFor(ablage).Y,
            roomX = ablage.Position.X,
            roomY = ablage.Position.Y,
            scale = Math.Round(BaseScale(distance) * _configuration.BubbleScaleFactor, 2),
            nameReadable = forceReadable || state is SpatialAblageBubbleState.ReadableBubble or SpatialAblageBubbleState.OpeningPortal or SpatialAblageBubbleState.PortalOpen or SpatialAblageBubbleState.ObjectEntering or SpatialAblageBubbleState.ObjectEmerging or SpatialAblageBubbleState.Placed,
            microTextVisible = distance == SpatialAblageDistance.Medium,
            openingText = state is SpatialAblageBubbleState.OpeningPortal or SpatialAblageBubbleState.PortalOpen or SpatialAblageBubbleState.ObjectEntering or SpatialAblageBubbleState.ObjectEmerging ? "Ablage oeffnet sich" : string.Empty,
            actionText = state is SpatialAblageBubbleState.PortalOpen or SpatialAblageBubbleState.ObjectEntering or SpatialAblageBubbleState.ObjectEmerging ? "Hier ablegen" : string.Empty
        };
    }

    private object CreateThingSnapshot(SpatialThing thing)
    {
        return new
        {
            thingId = thing.ThingId,
            displayName = thing.DisplayName,
            kind = thing.Kind,
            currentState = thing.CurrentState.ToString(),
            currentAblageId = thing.CurrentAblageId,
            currentCarryId = thing.CurrentCarryId,
            positionOnAblage = thing.PositionOnAblage,
            previewAblageId = thing.PreviewAblageId,
            metadata = thing.Metadata,
            updatedAt = thing.UpdatedAt
        };
    }

    private object CreateCarrySnapshot(SpatialCarrySession carry)
    {
        return new
        {
            carryId = carry.CarryId,
            thingId = carry.ThingId,
            sourceAblageId = carry.SourceAblageId,
            carrierAblageId = carry.CarrierAblageId,
            targetCandidateAblageId = carry.TargetCandidateAblageId,
            state = carry.State.ToString(),
            startedAt = carry.StartedAt,
            updatedAt = carry.UpdatedAt,
            position = carry.Position
        };
    }

    private object CreatePortalTransitionSnapshot(SpatialPortalTransition transition)
    {
        return new
        {
            transitionId = transition.TransitionId,
            thingId = transition.ThingId,
            sourceAblageId = transition.SourceAblageId,
            targetAblageId = transition.TargetAblageId,
            state = transition.State.ToString(),
            progress = transition.Progress,
            sourceVisualProgress = transition.SourceVisualProgress,
            targetVisualProgress = transition.TargetVisualProgress,
            startedAt = transition.StartedAt,
            updatedAt = transition.UpdatedAt
        };
    }

    private object CreateSurfaceThingSnapshot(string viewerAblageId, SpatialThing thing)
    {
        var isHere = thing.CurrentAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.RestingOnAblage or SpatialThingState.PlacedOnAblage or SpatialThingState.FreePlaced or SpatialThingState.Cancelled;
        var isCarriedHere = _room.ActiveCarry?.CarrierAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.Picked or SpatialThingState.Carried or SpatialThingState.PreviewOnAblage or SpatialThingState.ApproachingAblage;
        var isPreviewHere = thing.PreviewAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.PreviewOnAblage or SpatialThingState.ApproachingAblage;
        var portal = _room.ActivePortalTransition;
        var isPortalSource = portal?.SourceAblageId == viewerAblageId &&
            (thing.CurrentState is SpatialThingState.PreviewOnAblage or SpatialThingState.Carried);
        var isPortalTarget = portal?.TargetAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.PreviewOnAblage;
        var sourceWasHere = _room.ActiveCarry?.SourceAblageId == viewerAblageId &&
            thing.CurrentAblageId is null &&
            thing.CurrentCarryId is not null;
        return new
        {
            visible = isHere || isCarriedHere || isPreviewHere || sourceWasHere,
            isHere,
            isCarriedHere,
            isPreviewHere,
            ghostVisible = isPreviewHere || isPortalTarget,
            sourceWasHere,
            displayName = thing.DisplayName,
            state = thing.CurrentState.ToString(),
            position = isPreviewHere ? PreviewPosition(viewerAblageId) : thing.PositionOnAblage,
            displayScale = SurfaceThingScale(isHere, isCarriedHere, isPreviewHere, portal, viewerAblageId),
            glidePhase = SurfaceThingGlidePhase(isHere, isCarriedHere, isPreviewHere, sourceWasHere, portal, viewerAblageId),
            portalProgress = portal?.Progress ?? 0,
            sourceVisualProgress = isPortalSource ? portal?.SourceVisualProgress ?? 0 : 0,
            targetVisualProgress = isPortalTarget ? portal?.TargetVisualProgress ?? 0 : 0,
            readyToPlace = portal?.State == SpatialPortalTransitionState.ReadyToPlace && isPortalTarget,
            partialOcclusion = isCarriedHere,
            heldCompact = isCarriedHere,
            opticalHaptics = isCarriedHere || isPreviewHere,
            text = SurfaceThingText(isHere, isCarriedHere, isPreviewHere, sourceWasHere, thing)
        };
    }

    private string SurfaceThingText(
        bool isHere,
        bool isCarriedHere,
        bool isPreviewHere,
        bool sourceWasHere,
        SpatialThing thing)
    {
        if (isCarriedHere)
        {
            return "liegt in deiner Hand";
        }

        if (isPreviewHere)
        {
            return $"{thing.DisplayName} kommt an";
        }

        if (sourceWasHere)
        {
            return "Ding wurde genommen";
        }

        return isHere ? "liegt hier" : "bereit";
    }

    private SpatialAblageBubbleState BubbleStateFor(string ablageId, SpatialAblageDistance distance)
    {
        var thing = MainThing();
        if (thing.CurrentAblageId == ablageId &&
            thing.CurrentState is SpatialThingState.PlacedOnAblage or SpatialThingState.RestingOnAblage or SpatialThingState.FreePlaced)
        {
            return SpatialAblageBubbleState.Placed;
        }

        if (thing.PreviewAblageId == ablageId)
        {
            return PortalBubbleStateFor(ablageId);
        }

        return distance switch
        {
            SpatialAblageDistance.VeryFar => SpatialAblageBubbleState.DistantBubble,
            SpatialAblageDistance.Far => SpatialAblageBubbleState.DistantBubble,
            SpatialAblageDistance.Medium => SpatialAblageBubbleState.ApproachingBubble,
            _ => SpatialAblageBubbleState.ReadableBubble
        };
    }

    private SpatialAblageBubbleState PortalBubbleStateFor(string ablageId)
    {
        var transition = _room.ActivePortalTransition;
        if (transition is null || transition.TargetAblageId != ablageId)
        {
            return SpatialAblageBubbleState.OpeningPortal;
        }

        return transition.State switch
        {
            SpatialPortalTransitionState.Entering => SpatialAblageBubbleState.ObjectEntering,
            SpatialPortalTransitionState.InBetween => SpatialAblageBubbleState.ObjectEntering,
            SpatialPortalTransitionState.Emerging => SpatialAblageBubbleState.ObjectEmerging,
            SpatialPortalTransitionState.ReadyToPlace => SpatialAblageBubbleState.ObjectEmerging,
            SpatialPortalTransitionState.Placed => SpatialAblageBubbleState.Placed,
            _ => SpatialAblageBubbleState.PortalOpen
        };
    }

    private SpatialAblageDistance DistanceFor(string viewerAblageId, string targetAblageId)
    {
        if (targetAblageId == "monitor" && viewerAblageId == "handy")
        {
            return SpatialAblageDistance.VeryNear;
        }

        if (targetAblageId == "handy" && viewerAblageId == "monitor")
        {
            return SpatialAblageDistance.VeryNear;
        }

        if ((targetAblageId == "monitor" && viewerAblageId == "tablet") ||
            (targetAblageId == "tablet" && viewerAblageId == "monitor"))
        {
            return SpatialAblageDistance.VeryNear;
        }

        if (targetAblageId == "tablet")
        {
            return SpatialAblageDistance.Near;
        }

        if (targetAblageId == "desktop")
        {
            return SpatialAblageDistance.Medium;
        }

        if (targetAblageId == "beamer")
        {
            return SpatialAblageDistance.Far;
        }

        return SpatialAblageDistance.Medium;
    }

    private SpatialAblage FindAblageOrDefault(string? ablageId)
    {
        return _room.Ablagen.FirstOrDefault(ablage =>
                string.Equals(ablage.AblageId, ablageId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ablage.DisplayName, ablageId, StringComparison.OrdinalIgnoreCase))
            ?? _room.Ablagen.First(ablage => ablage.AblageId == DefaultAblageId);
    }

    private SpatialThing MainThing()
    {
        return _room.Things.First(thing => thing.ThingId == ThingId);
    }

    private void ReplaceThing(SpatialThing updatedThing)
    {
        _room = _room with
        {
            Things = _room.Things.Select(thing => thing.ThingId == updatedThing.ThingId ? updatedThing : thing).ToArray()
        };
    }

    private string CarryStateText()
    {
        return _room.ActiveCarry?.State switch
        {
            SpatialCarrySessionState.Picked => "Picked",
            SpatialCarrySessionState.Carried => "Carried",
            SpatialCarrySessionState.NearAblage => "NearAblage",
            SpatialCarrySessionState.OpeningAblage => "OpeningAblage",
            SpatialCarrySessionState.PreviewingOnAblage => "PreviewingOnAblage",
            SpatialCarrySessionState.Placed => "Placed",
            SpatialCarrySessionState.Cancelled => "Cancelled",
            _ => "OnTray"
        };
    }

    private string StatusText(string viewerAblageId, SpatialThing thing)
    {
        if (_room.ActiveCarry?.CarrierAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.Carried or SpatialThingState.PreviewOnAblage)
        {
            return thing.CurrentState == SpatialThingState.PreviewOnAblage
                ? "Ablage oeffnet sich"
                : "Ding liegt in deiner Hand";
        }

        if (thing.PreviewAblageId == viewerAblageId)
        {
            return $"{thing.DisplayName} kommt an";
        }

        if (thing.CurrentAblageId == viewerAblageId)
        {
            return thing.CurrentState == SpatialThingState.FreePlaced ? "Ding liegt jetzt hier" : "Liegt jetzt hier";
        }

        if (_room.ActiveCarry?.SourceAblageId == viewerAblageId && thing.CurrentAblageId is null)
        {
            return "Ding wurde genommen";
        }

        return "Ablage bereit";
    }

    private static string PlacementKind(SpatialThing thing)
    {
        return thing.CurrentState switch
        {
            SpatialThingState.FreePlaced => "Free",
            SpatialThingState.PlacedOnAblage => "Ablage",
            SpatialThingState.RestingOnAblage => "Ablage",
            SpatialThingState.Carried => "Hand",
            SpatialThingState.Picked => "Hand",
            SpatialThingState.PreviewOnAblage => "Preview",
            _ => "Raum"
        };
    }

    private static SpatialPoint PlacementPoint(double? x, double? y)
    {
        var rawX = x ?? 0.5;
        var rawY = y ?? 0.5;

        if (Math.Abs(rawX) > 1 || Math.Abs(rawY) > 1)
        {
            rawX = 0.5 + (rawX / 720);
            rawY = 0.5 + (rawY / 720);
        }

        return new SpatialPoint(Clamp01(rawX), Clamp01(rawY));
    }

    private static SpatialPoint PreviewPosition(string viewerAblageId)
    {
        return viewerAblageId switch
        {
            "monitor" => new SpatialPoint(0.62, 0.46),
            "handy" => new SpatialPoint(0.48, 0.52),
            "tablet" => new SpatialPoint(0.54, 0.48),
            _ => new SpatialPoint(0.5, 0.5)
        };
    }

    private static double SurfaceThingScale(
        bool isHere,
        bool isCarriedHere,
        bool isPreviewHere,
        SpatialPortalTransition? portal,
        string viewerAblageId)
    {
        if (portal?.TargetAblageId == viewerAblageId && isPreviewHere)
        {
            return Math.Round(0.62 + (portal.TargetVisualProgress * 0.38), 2);
        }

        if (portal?.SourceAblageId == viewerAblageId && isCarriedHere)
        {
            return Math.Round(0.92 - (portal.SourceVisualProgress * 0.14), 2);
        }

        if (isPreviewHere)
        {
            return 0.74;
        }

        if (isCarriedHere)
        {
            return 0.94;
        }

        return isHere ? 1.0 : 0.9;
    }

    private static string SurfaceThingGlidePhase(
        bool isHere,
        bool isCarriedHere,
        bool isPreviewHere,
        bool sourceWasHere,
        SpatialPortalTransition? portal,
        string viewerAblageId)
    {
        if (portal?.SourceAblageId == viewerAblageId && isCarriedHere)
        {
            return "ObjectEntering";
        }

        if (portal?.TargetAblageId == viewerAblageId && isPreviewHere)
        {
            return portal.State == SpatialPortalTransitionState.ReadyToPlace
                ? "ReadyToPlace"
                : "ObjectEmerging";
        }

        if (isPreviewHere)
        {
            return "Arriving";
        }

        if (isCarriedHere)
        {
            return "Held";
        }

        if (sourceWasHere)
        {
            return "Lifted";
        }

        return isHere ? "Placed" : "None";
    }

    private static double Clamp01(double value)
    {
        return Math.Max(0, Math.Min(1, value));
    }

    private static SpatialPoint EdgePositionFor(SpatialAblage ablage)
    {
        return ablage.AblageId switch
        {
            "monitor" => new SpatialPoint(0.92, 0.48),
            "handy" => new SpatialPoint(0.08, 0.52),
            "tablet" => new SpatialPoint(0.82, 0.16),
            "desktop" => new SpatialPoint(0.08, 0.46),
            "beamer" => new SpatialPoint(0.54, 0.08),
            _ => ablage.Position
        };
    }

    private static double BaseScale(SpatialAblageDistance distance)
    {
        return distance switch
        {
            SpatialAblageDistance.VeryFar => 0.36,
            SpatialAblageDistance.Far => 0.52,
            SpatialAblageDistance.Medium => 0.72,
            SpatialAblageDistance.Near => 0.94,
            SpatialAblageDistance.VeryNear => 1.1,
            _ => 0.72
        };
    }
}
