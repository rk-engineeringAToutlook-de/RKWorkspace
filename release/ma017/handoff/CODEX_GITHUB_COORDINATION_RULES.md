# RK Workspace - Codex GitHub Coordination Rules

## Status

Normative coordination note for parallel Codex instances.

This file exists because Windows Codex and macOS Codex may both run in user/HiDrive folders.
HiDrive must not become the coordination mechanism for source changes.

## Golden Rule

```text
Codex instances exchange project state only through GitHub.
```

Do not rely on:

- HiDrive file sync as source exchange.
- copying changed files manually between machines.
- editing the same uncommitted working tree from two machines.
- assuming another Codex instance can see local uncommitted files.

## Required Workflow

Before starting work:

```bash
git fetch origin
git status
git pull --ff-only
```

During work:

- Work on the branch assigned to the current task.
- Keep changes focused.
- Do not edit files owned by the other active Codex instance unless explicitly requested.
- Do not depend on uncommitted files from another machine.

After a finished, verified slice:

```bash
git status
git add <intentional files>
git commit -m "<message>"
git push
```

Before continuing after another instance has pushed:

```bash
git fetch origin
git pull --ff-only
```

## Branch Rule

For the current MA017 macOS PDF Frame pilot, both instances use:

```text
feature/ma016-ma017-real-platform-pilot
```

Only one instance should push a slice at a time.

If both instances need to develop at the same time, create separate task branches:

```text
feature/ma017-windows-owner-frame
feature/ma017-macos-frame-guest
```

Then merge through GitHub or a deliberate local merge.

## Dirty Working Trees

If `git status` shows uncommitted changes:

- Do not pull blindly.
- Do not overwrite.
- First identify whether the changes belong to the current task.
- Commit, stash, or explicitly leave them untouched.

Current rule:

```text
Uncommitted local changes are not shared state.
Only pushed commits are shared state.
```

## HiDrive Rule

HiDrive may store working directories, but it must not be treated as the source of truth.

If HiDrive sync creates conflicts, duplicate files, lock files, or partial states:

- stop work
- inspect `git status`
- resolve using Git
- do not manually merge random synced files

## macOS Codex Rule

The macOS Codex instance must begin by reading:

```text
release/ma017/handoff/MACOS_CODEX_REAL_PDF_FRAME_START_HERE.md
release/ma017/handoff/CODEX_GITHUB_COORDINATION_RULES.md
```

It must pull from GitHub before building.

## Windows Codex Rule

The Windows Codex instance must push only intentional completed slices that macOS needs.

Local experiments, generated reports, and unrelated working-tree changes must not be staged into handoff commits.

## Definition Of Safe Exchange

The exchange is safe only when:

- the producing instance committed and pushed
- the consuming instance fetched and pulled
- both instances know the branch name
- no one depends on uncommitted HiDrive-synced files

## Current Shared Branch

```text
origin/feature/ma016-ma017-real-platform-pilot
```

Current handoff purpose:

```text
Windows owns the real PDF.
macOS displays only the PDF frame.
No File Ingress remains mandatory.
```
