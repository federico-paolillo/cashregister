# Diary

> This file records implementation decisions, design choices, and strategies per task to avoid re-deriving the same conclusions when picking up work later.

## Technician device selection page

Added a `/devices` frontend route and backend device endpoints for selecting the receipt printer target at runtime. The backend exposes `/devices` only; `/api` remains a frontend proxy/base-url concern. Device ids are URL-safe identifiers derived from writable Linux printer file paths because the current `FileDevice` writes bytes through `FileStream`.

### Key decisions

- We keep selected target state in a singleton runtime store initialized from `FileDeviceSettings.Target`. We do not mutate the Options cache because options binding is configuration input, not runtime application state.
- We validate selection ids against the current device catalog before updating the runtime target. We do not accept arbitrary path strings from the client.
- We enumerate `/dev/usb/lp*` and `/dev/lp*` file devices instead of CUPS queues because CUPS URIs are not valid `FileStream` targets.
- We use `/dev/null` as the development default target because `FileDevice` opens an existing file path for writing.

## Receipt PrintProgram template service

Added an Application-layer receipt service that builds a `PrintProgram` for an order from a receipt-specific projection. The projection contains only order number, id, date, item descriptions, and quantities; it deliberately excludes prices and totals. The service is pure and only constructs the print template, leaving actual printer I/O for later order-submission orchestration.

### Key decisions

- We return `Result<PrintProgram>` and `NoSuchOrderPrintDataProblem` for missing orders because expected Application failures use `Result<T>` and `Problem`, not exceptions.
- We project receipt data directly from EF entities into Application output models because receipt construction should not depend on Domain aggregate models.
- We format receipt dates as UTC because `TimeStamp` stores Unix seconds generated from UTC time.

## Stable receipt item order

Fixed receipt projection item ordering so printed receipt lines are deterministic. The projection now orders order items before materializing receipt print data, and the integration test asserts the item sequence instead of only checking membership.

### Key decisions

- We order receipt items by their persisted item id because order items currently have no explicit line-number column. This keeps receipt output stable without changing the schema.
