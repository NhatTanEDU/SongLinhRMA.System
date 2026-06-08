# Task Plan: Background Alert Scanner (UC03)

## Goal
Implement a background hosted service in RMA.Server that scans Firestore periodically (every 1 hour), calculates alert levels (Green, Yellow, Red) based on the age of sent RMA tickets, updates their priority and alert color in Firestore, and sends FCM push notifications for critical tickets.

## Current Phase
Phase 5: Handoff

## Phases

### Phase 1: Requirements & Discovery
- [x] Understand user requirements and constraints
- [x] Review current entities (RmaTicket, StatusMaster) and DTOs
- [x] Check FirestoreRepository query capabilities and FCM service setup
- **Status:** complete

### Phase 2: Design & Planning
- [x] Create implementation plan in artifacts and request user feedback
- [x] Define technical changes to RmaTicket (adding WarningColor) and mapping in RmaTicketDto
- [x] Design background service execution loop and scope lifetime management
- **Status:** complete

### Phase 3: Implementation
- [x] Create RmaAlertBackgroundService.cs in RMA.Server/Services
- [x] Add WarningColor to RmaTicket.cs and RmaTicketDto.cs
- [x] Update IFcmService.cs and FcmService.cs to accept string ticketId
- [x] Update RmaTicketsController.cs mapping
- [x] Adjust UI in RmaTickets.razor to render the warning colors
- [x] Register service in Program.cs
- **Status:** complete

### Phase 4: Testing & Verification
- [x] Build and compile the project
- [x] Verify background service starts up successfully
- [x] Manually test or simulate ticket updates to check alert levels and FCM logs
- **Status:** complete

### Phase 5: Handoff
- [x] Prepare walkthrough.md summary
- [x] Present changes to the user
- **Status:** complete

## Key Questions
1. Should `WarningColor` be stored in the Firestore database? (Yes, so it can be queried and displayed by the client)
2. How is "incomplete status" determined? (Any status other than "Closed")
3. What color codes should be saved? ("Green" for < 10 days, "Yellow" for 10-13 days, "Red" for >= 14 days)

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| Add `WarningColor` property to `RmaTicket` | Allows persistency of the scanner's state and lets the Blazor client display alert levels |
| Change FCM ticketId parameter to string | Firestore uses string document IDs, so having FCM accept string prevents conversion mismatch |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
|       | 1       |            |
