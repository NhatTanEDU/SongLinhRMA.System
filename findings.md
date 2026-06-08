# Findings & Decisions - Background Alert Scanner (UC03)

## Requirements
- Create `RmaAlertBackgroundService.cs` (inheriting from `BackgroundService`) in `RMA.Server`.
- Periodic loop running every 1 hour (using `IServiceScopeFactory` to resolve Scoped services like `FirestoreRepository<T>`).
- Query Firestore: Get all `RMA_TICKETS` in an incomplete status with non-null `sent_date`.
- Compare current date and `sent_date`:
  - < 10 days: Green (Xanh)
  - 10-13 days: Yellow (Vàng)
  - >= 14 days: Red (Đỏ) - Update priority (`IsUrgent = true`) and send FCM warning.
- Register background service via `services.AddHostedService<RmaAlertBackgroundService>();` in `Program.cs`.

## Research Findings
- **Firestore IDs**: Firestore document IDs in this project are represented as `string` properties mapped with `[FirestoreDocumentId]`.
- **FCM Service**: `FcmService.cs` takes `int ticketId` in `SendAlertAsync`. This needs to be changed to `string ticketId` since Firestore IDs are strings.
- **Repository Scope**: `FirestoreRepository<T>` is registered as Scoped. BackgroundService is Singleton. Hence, using `IServiceScopeFactory` is mandatory to resolve repositories inside the loop.
- **Statuses**: Default statuses in the database are: `New` (Blue), `In Progress` (Orange), `Waiting for Parts` (Red), `Repaired` (Green), `Closed` (Gray). "Incomplete" represents all statuses except `Closed`.

## Technical Decisions
| Decision | Rationale |
|----------|-----------|
| Add `WarningColor` to `RmaTicket` and `RmaTicketDto` | Allows persistency of the alert levels so the UI can render them |
| Change `IFcmService.SendAlertAsync` ticketId to string | Firestore document IDs are strings, so FCM service needs to match this type |

## Issues Encountered
| Issue | Resolution |
|-------|------------|
|       |            |

## Resources
- BackgroundService doc: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
- Firestore .NET SDK docs: https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest
