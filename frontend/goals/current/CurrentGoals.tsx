import {
  normalizeFundNames,
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import CurrentGoalsFilter from "@/goals/current/CurrentGoalsFilter";
import CurrentGoalsList from "@/goals/current/CurrentGoalsList";
import type { CurrentGoals as CurrentGoalsModel } from "@/goals/types";
import CurrentGoalsSummaryCards from "@/goals/current/CurrentGoalsSummaryCards";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";

interface CurrentGoalsSearchParams {
  fundName?: string | readonly string[];
}

interface CurrentGoalsProps {
  readonly searchParams: Promise<CurrentGoalsSearchParams>;
}

const createEmptyCurrent = function (): CurrentGoalsModel {
  return {
    accountingPeriodId: null,
    accountingPeriodName: null,
    availableFundNames: [],
    summary: {
      totalAmountToAssign: 0,
      totalAmountAssigned: 0,
      percentageOfAssignmentGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
      totalAmountToSpend: 0,
      totalAmountSpent: 0,
      percentageOfSpendingGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
    },
    goals: [],
  };
};

/**
 * Component that displays the current Goals snapshot.
 */
const CurrentGoals = async function ({
  searchParams,
}: CurrentGoalsProps): Promise<JSX.Element> {
  const { fundName } = await searchParams;
  const currentFundNames = normalizeRequestedFundNames(
    Array.isArray(fundName)
      ? fundName
      : typeof fundName === "string"
        ? [fundName]
        : [],
  );

  const apiClient = getApiClient();
  const current: CurrentGoalsModel =
    (
      await apiClient.GET("/goals/current", {
        params: {
          query: {
            ...(shouldPersistFundNames(currentFundNames)
              ? { FundName: [...currentFundNames] }
              : {}),
          },
        },
      })
    ).data ?? createEmptyCurrent();

  const availableFundNames = normalizeFundNames(
    current.availableFundNames,
    current.availableFundNames,
  );

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <CurrentGoalsFilter availableFundNames={availableFundNames} />
      </Stack>
      <CurrentGoalsSummaryCards current={current} />
      <CurrentGoalsList current={current} />
    </Stack>
  );
};

export type { CurrentGoalsSearchParams };
export default CurrentGoals;
