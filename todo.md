I've recently updated a lot of the frontend account components and now I want to make similar changes for funds.

1. Move the create/onboard fund forms to their own pages
    1. Use the shared Frame component when possible to allow the forms to use the same design language as the rest of the application

1. Create a view fund form that displays information about the fund including current balance and recent balance events

1. Move the update and delete fund forms to just become dialogs. These dialogs should be very similar to the UpdateAccountForm and DeleteAccountForm.

1. Remove the split layout from the FundWorkspace. The workspace should just display a header and the list frame of funds.

1. Replace the list frame presentation of the funds in the workspace with a card layout. These cards should be very similar to the AccountWorkspaceCards.

1. Remove the "current" fund view and move the fund workspace up in the navigational hierarchy.