I'm working on transforming the "views" in my application into fully realized dashboards with top level search and filters that control the information available to all components in the dashboard. To start implementing this, I want to design all-in-one dashboard endpoints that support the data that I'm going to need.

1. Account Dashboard Endpoint

    This endpoint should take in the following parameters:
    1. An optional range of dates
    1. An optional range of accounting periods
    1. An optional account type filter
    1. An optional search term
    1. An optional sort model
    1. An optional page number

    The model returned by this endpoint needs to include the following information:
    1. A paginated collection of accounts that match the search criteria
    1. The total balance for all accounts for each date / accounting period included in the search range
    1. The total balance by tracked / untracked for each date / accounting period included in the search range
    1. The total balance by account type for each date / accounting period included in the search range

    If the caller provides a range of accounting periods, the opening and closing balances for each accounting period in the range should be included.

    Either the range of dates or the range of accounting periods must be provided.

1. Fund Dashboard Endpoint

    This endpoint should take in the following parameters:
    1. An optional range of dates
    1. An optional range of accounting periods
    1. An optional search term
    1. An optional sort model
    1. An optional page number

    The model returned by this endpoint needs to include the following information:
    1. A paginated collection of funds that match the search criteria
    1. The total balance for all funds for each date / accounting period included in the search range
    1. The total balance by assigned / unassigned for each date / accounting period included in the search range

    If the caller provides a range of accounting periods, the opening and closing balances for each accounting period in the range should be included.

    Either the range of dates or the range of accounting periods must be provided.