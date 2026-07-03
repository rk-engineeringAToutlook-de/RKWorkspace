namespace RKWorkspace.Shell.SpatialTray;

public sealed class SpatialTraySession
{
    private const string RoomId = "rkws-spatial-room-local";
    private const string ThingId = "thing-rechnung";
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
                    CurrentAblageId = "handy",
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
            var viewer = FindAblageOrDefault(surfaceAblageId ?? "handy");
            var thing = MainThing();
            var bubbles = CreateBubbles(viewer.AblageId).ToArray();
            var surfaceThing = CreateSurfaceThingSnapshot(viewer.AblageId, thing);
            return new
            {
                roomId = _room.RoomId,
                version = _room.Version,
                updatedAt = _room.UpdatedAt,
                state = CurrentState.ToString(),
                carryState = CarryStateText(),
                surface = CreateAblageSnapshot(viewer, SpatialAblageBubbleState.Readable, viewer.Distance, true),
                ablagen = _room.Ablagen.Select(ablage => CreateAblageSnapshot(ablage, SpatialAblageBubbleState.Visible, ablage.Distance, true)).ToArray(),
                things = _room.Things.Select(CreateThingSnapshot).ToArray(),
                thing = "Digitales Ding",
                name = thing.DisplayName,
                activeCarry = _room.ActiveCarry is null ? null : CreateCarrySnapshot(_room.ActiveCarry),
                activeAblage = viewer.DisplayName,
                activeAblageId = viewer.AblageId,
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
                    targetGhostBeforePlace = true,
                    glideIntoBubble = true,
                    placeAfterGlide = true,
                    targetPositioning = "relative-on-ablage",
                    carriedScale = _configuration.HeldCompactScale,
                    previewScale = 0.82,
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
        Pick("handy");
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
            _room = _room with { ActiveCarry = carry, UpdatedAt = now };
        }
    }

    public void Carry()
    {
        Move("handy", null, null);
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
        Approach("handy", ablage);
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
            ReplaceThing(MainThing() with
            {
                CurrentState = SpatialThingState.PreviewOnAblage,
                CurrentAblageId = null,
                CurrentCarryId = carry.CarryId,
                PreviewAblageId = target.AblageId,
                PositionOnAblage = null,
                UpdatedAt = now
            });
            _room = _room with { ActiveCarry = carry, UpdatedAt = now };
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
            var target = FindAblageOrDefault(ablage ?? _room.ActiveCarry?.TargetCandidateAblageId ?? "monitor");
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
            isAvailable = ablage.IsAvailable,
            isActive = state is SpatialAblageBubbleState.Opening or SpatialAblageBubbleState.Active,
            opens = state is SpatialAblageBubbleState.Opening or SpatialAblageBubbleState.Active,
            canReceive = ablage.CanReceive,
            canProvide = ablage.CanProvide,
            metadata = ablage.Metadata,
            lastSeen = ablage.LastSeen,
            x = ablage.Position.X,
            y = ablage.Position.Y,
            scale = Math.Round(BaseScale(distance) * _configuration.BubbleScaleFactor, 2),
            nameReadable = forceReadable || state is SpatialAblageBubbleState.Readable or SpatialAblageBubbleState.Opening or SpatialAblageBubbleState.Active or SpatialAblageBubbleState.Placed,
            microTextVisible = distance == SpatialAblageDistance.Medium,
            openingText = state is SpatialAblageBubbleState.Opening or SpatialAblageBubbleState.Active ? "Ablage oeffnet sich" : string.Empty,
            actionText = state is SpatialAblageBubbleState.Opening or SpatialAblageBubbleState.Active ? "Hier ablegen" : string.Empty
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

    private object CreateSurfaceThingSnapshot(string viewerAblageId, SpatialThing thing)
    {
        var isHere = thing.CurrentAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.RestingOnAblage or SpatialThingState.PlacedOnAblage or SpatialThingState.FreePlaced or SpatialThingState.Cancelled;
        var isCarriedHere = _room.ActiveCarry?.CarrierAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.Picked or SpatialThingState.Carried or SpatialThingState.PreviewOnAblage or SpatialThingState.ApproachingAblage;
        var isPreviewHere = thing.PreviewAblageId == viewerAblageId &&
            thing.CurrentState is SpatialThingState.PreviewOnAblage or SpatialThingState.ApproachingAblage;
        var sourceWasHere = _room.ActiveCarry?.SourceAblageId == viewerAblageId &&
            thing.CurrentAblageId is null &&
            thing.CurrentCarryId is not null;
        return new
        {
            visible = isHere || isCarriedHere || isPreviewHere || sourceWasHere,
            isHere,
            isCarriedHere,
            isPreviewHere,
            ghostVisible = isPreviewHere,
            sourceWasHere,
            displayName = thing.DisplayName,
            state = thing.CurrentState.ToString(),
            position = isPreviewHere ? PreviewPosition(viewerAblageId) : thing.PositionOnAblage,
            displayScale = SurfaceThingScale(isHere, isCarriedHere, isPreviewHere),
            glidePhase = SurfaceThingGlidePhase(isHere, isCarriedHere, isPreviewHere, sourceWasHere),
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
            return SpatialAblageBubbleState.Opening;
        }

        return distance switch
        {
            SpatialAblageDistance.VeryFar => SpatialAblageBubbleState.Visible,
            SpatialAblageDistance.Far => SpatialAblageBubbleState.Visible,
            SpatialAblageDistance.Medium => SpatialAblageBubbleState.Approaching,
            _ => SpatialAblageBubbleState.Readable
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
            ?? _room.Ablagen.First(ablage => ablage.AblageId == "handy");
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
            _ => new SpatialPoint(0.5, 0.5)
        };
    }

    private static double SurfaceThingScale(bool isHere, bool isCarriedHere, bool isPreviewHere)
    {
        if (isPreviewHere)
        {
            return 0.82;
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
        bool sourceWasHere)
    {
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
