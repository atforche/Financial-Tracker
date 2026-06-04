import {
  AccountingPeriodGoalSortOrder,
  type AccountingPeriodIdentifier,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import GoalListFrame from "@/goals/GoalListFrame";
import GoalsDashboardControls from "@/goals/GoalsDashboardControls";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the GoalsView component.
 */
interface GoalsViewSearchParams {
  accountingPeriodId?: string | null;
  search?: string | null;
  sort?: AccountingPeriodGoalSortOrder | null;
  page?: number | null;
}

/**
 * Props for the GoalsView component.
 */
interface GoalsViewProps {
  readonly searchParams: Promise<GoalsViewSearchParams>;
}

/**
 * Component that displays the top-level Goals workspace.
 */
const GoalsView = async function ({
  searchParams,
}: GoalsViewProps): Promise<JSX.Element> {
  const { accountingPeriodId, search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: 250,
        Offset: 0,
      },
    },
  });
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );

  const [{ data: accountingPeriods }, { data: openAccountingPeriods }] =
    await Promise.all([accountingPeriodsPromise, openAccountingPeriodsPromise]);

  if (
    typeof accountingPeriods === "undefined" ||
    typeof openAccountingPeriods === "undefined"
  ) {
    throw new Error("Failed to fetch goals dashboard filters");
  }

  const selectedAccountingPeriod =
    typeof accountingPeriodId === "string"
      ? (accountingPeriods.items.find(
          (accountingPeriod) => accountingPeriod.id === accountingPeriodId,
        ) ?? null)
      : (openAccountingPeriods[0] ?? accountingPeriods.items[0] ?? null);
  const currentOpenAccountingPeriod = openAccountingPeriods[0] ?? null;

  const goalsResponse =
    selectedAccountingPeriod === null
      ? null
      : await apiClient.GET("/accounting-periods/{accountingPeriodId}/goals", {
          params: {
            path: {
              accountingPeriodId: selectedAccountingPeriod.id,
            },
            query: {
              Search: search ?? "",
              Sort: sort ?? null,
              Limit: rowsPerPage,
              Offset: ((page ?? 1) - 1) * rowsPerPage,
            },
          },
        });

  if (
    selectedAccountingPeriod !== null &&
    typeof goalsResponse?.data === "undefined"
  ) {
    throw new Error(
      `Failed to fetch goals for accounting period ${selectedAccountingPeriod.id}`,
    );
  }

  const goals =
    selectedAccountingPeriod === null ||
    typeof goalsResponse?.data === "undefined"
      ? { items: [], totalCount: 0 }
      : goalsResponse.data;

  const currentSearch = search?.trim() ?? "";
  const hasActiveSearch = currentSearch !== "";
  const visibleCount = goals.items.length;
  const accountingPeriodOptions: AccountingPeriodIdentifier[] =
    accountingPeriods.items.map((accountingPeriod) => ({
      id: accountingPeriod.id,
      name: accountingPeriod.name,
    }));
  const assignmentGoalMetCount = goals.items.filter(
    (goal) => goal.isAssignmentGoalMet,
  ).length;
  const spendingGoalMetCount = goals.items.filter(
    (goal) => goal.isSpendingGoalMet,
  ).length;
  const remainingAmountToAssign = goals.items.reduce(
    (sum, goal) => sum + goal.remainingAmountToAssign,
    0,
  );
  const remainingAmountToSpend = goals.items.reduce(
    (sum, goal) => sum + goal.remainingAmountToSpend,
    0,
  );
  const createActionHref = routes.create();
  const createActionLabel =
    selectedAccountingPeriod?.isOpen === true
      ? "Create goal"
      : currentOpenAccountingPeriod !== null
        ? "Create goal in current period"
        : "Open accounting period";

  if (selectedAccountingPeriod === null) {
    return (
      <Stack spacing={3} sx={{ maxWidth: 1440 }}>
        <Paper
          sx={{
            backgroundColor: "background.paper",
            backgroundImage:
              "linear-gradient(135deg, rgba(255, 193, 7, 0.16) 0%, rgba(76, 175, 80, 0.12) 48%, rgba(255, 255, 255, 0) 74%)",
            border: "1px solid",
            borderColor: "divider",
            overflow: "hidden",
            p: { xs: 3, md: 4 },
          }}
        >
          <Stack spacing={2.5} maxWidth={720}>
            <Typography variant="overline" color="text.secondary">
              Goals workspace
            </Typography>
            <Typography variant="h3">Goals dashboard</Typography>
            <Typography color="text.secondary">
              Create an accounting period before tracking assignment or spending
              targets across your funds.
            </Typography>
          </Stack>
        </Paper>
      </Stack>
    );
  }

  return (
    <Stack spacing={3} sx={{ maxWidth: 1440 }}>
      <Paper
        sx={{
          backgroundColor: "background.paper",
          backgroundImage:
            "linear-gradient(135deg, rgba(255, 193, 7, 0.18) 0%, rgba(76, 175, 80, 0.14) 46%, rgba(255, 255, 255, 0) 74%)",
          border: "1px solid",
          borderColor: "divider",
          overflow: "hidden",
          p: { xs: 3, md: 4 },
        }}
      >
        <Box
          sx={{
            display: "grid",
            gap: 3,
            gridTemplateColumns: {
              xs: "1fr",
              xl: "minmax(0, 1.2fr) minmax(380px, 0.8fr)",
            },
          }}
        >
          <Stack spacing={3}>
            <Stack spacing={1}>
              <Typography variant="overline" color="text.secondary">
                Goals workspace
              </Typography>
              <Typography variant="h3">Goals dashboard</Typography>
              <Typography color="text.secondary" maxWidth={760}>
                Review fund targets for the selected period, find goals that
                still need assignment or spending attention, and move into goal
                detail without leaving the workspace.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {hasActiveSearch
                  ? `Showing ${visibleCount} of ${goals.totalCount} goals in ${selectedAccountingPeriod.name} matching "${currentSearch}".`
                  : `Showing ${visibleCount} goals on this page in ${selectedAccountingPeriod.name} across ${goals.totalCount} total goals.`}
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              <Button variant="contained" href={createActionHref}>
                {createActionLabel}
              </Button>
              {currentOpenAccountingPeriod !== null &&
              currentOpenAccountingPeriod.id !== selectedAccountingPeriod.id ? (
                <Button
                  variant="outlined"
                  href={routes.index({
                    accountingPeriodId: currentOpenAccountingPeriod.id,
                  })}
                >
                  Current period
                </Button>
              ) : null}
              <Button
                variant="outlined"
                href={routes.index({
                  accountingPeriodId: selectedAccountingPeriod.id,
                  sort: AccountingPeriodGoalSortOrder.RemainingAmountToAssignDescending,
                })}
              >
                Highest assignment gaps
              </Button>
            </Stack>
          </Stack>
          <GoalsDashboardControls
            accountingPeriods={accountingPeriodOptions}
            accountingPeriodParamName={nameof<GoalsViewSearchParams>(
              "accountingPeriodId",
            )}
            searchParamName={nameof<GoalsViewSearchParams>("search")}
            sortParamName={nameof<GoalsViewSearchParams>("sort")}
            pageParamName={nameof<GoalsViewSearchParams>("page")}
          />
        </Box>
      </Paper>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(2, minmax(0, 1fr))",
            xl: "repeat(4, minmax(0, 1fr))",
          },
        }}
      >
        <SummaryCard
          title="Goals In Scope"
          value={goals.totalCount}
          description={`${visibleCount} visible on this page for ${selectedAccountingPeriod.name}.`}
        />
        <SummaryCard
          title="Assignment Goals Met"
          value={assignmentGoalMetCount}
          description="Visible goals whose assignment target is already met."
        />
        <SummaryCard
          title="Remaining To Assign"
          value={formatCurrency(remainingAmountToAssign)}
          description="Sum across visible goals on this page."
        />
        <SummaryCard
          title="Remaining To Spend"
          value={formatCurrency(remainingAmountToSpend)}
          description={`${spendingGoalMetCount} visible goal${spendingGoalMetCount === 1 ? "" : "s"} already met on spending.`}
        />
      </Box>
      <Stack spacing={2}>
        <Stack spacing={0.5}>
          <Typography variant="h5">
            Goals in {selectedAccountingPeriod.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Use the workspace controls to change scope or sort, then open a goal
            for fund-level detail and transactions.
          </Typography>
        </Stack>
        <GoalListFrame
          accountingPeriod={selectedAccountingPeriod}
          data={goals.items}
          totalCount={goals.totalCount}
          searchParamName={nameof<GoalsViewSearchParams>("search")}
          sortParamName={nameof<GoalsViewSearchParams>("sort")}
          pageParamName={nameof<GoalsViewSearchParams>("page")}
        />
      </Stack>
    </Stack>
  );
};

export type { GoalsViewSearchParams };
export default GoalsView;
