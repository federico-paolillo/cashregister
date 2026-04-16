# Diary

> This file records implementation decisions, design choices, and strategies per task to avoid re-deriving the same conclusions when picking up work later.

## Technician device selection page

Added a `/devices` frontend route and backend device endpoints for selecting the receipt printer target at runtime. The backend exposes `/devices` only; `/api` remains a frontend proxy/base-url concern. Device ids are URL-safe identifiers derived from writable Linux printer file paths because the current `FileDevice` writes bytes through `FileStream`.

### Key decisions

- We keep selected target state in a singleton runtime store initialized from `FileDeviceSettings.Target`. We do not mutate the Options cache because options binding is configuration input, not runtime application state.
- We validate selection ids against the current device catalog before updating the runtime target. We do not accept arbitrary path strings from the client.
- We enumerate `/dev/usb/lp*` and `/dev/lp*` file devices instead of CUPS queues because CUPS URIs are not valid `FileStream` targets.
- We use `/dev/null` as the development default target because `FileDevice` opens an existing file path for writing.
