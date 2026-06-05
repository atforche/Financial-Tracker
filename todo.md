1. Create the GoalDashboardModel and endpoint
    1. Create the GoalDashboardQueryParameterModel that includes a start accounting period, end accounting period, collection of goal types, collection of fund names, a sort, a limit, and an offset.

    1. Create a GoalDashboardModel that includes:
        1. A paginated collection of goals
        1. A collection of available fund names
        1. the total goal amount
            1. Total across all funds and accounting periods
            1. Total across each fund type and all accounting periods
            1. Total across all funds for each accounting period
        1. the total amount assigned
            1. Total across all funds and accounting periods
            1. Total across each fund type and all accounting periods
            1. Total across all funds for each accounting period
        1. the total amount spent
            1. Total across all funds and accounting periods
            1. Total across each fund type and all accounting periods
            1. Total across all funds for each accounting period
        1. the percentage of goals met
            1. Percentage across all funds and accounting periods
            1. Percentage across each fund type and all accounting periods
            1. Percentage across all funds for each accounting period

    1. Create a GoalDashboardGetter model that is responsible for populating the GoalDashboardModel

    1. Add an endpoint to the goal controller