import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import CurrentFundsFilter from "@/funds/current/CurrentFundsFilter";
import CurrentFundsList from "@/funds/current/CurrentFundsList";
import type { CurrentFunds as CurrentFundsModel } from "@/funds/types";
import CurrentFundsSummaryCard from "@/funds/current/CurrentFundsSummaryCard";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";

interface CurrentFundsSearchParams {
  fundName?: string | readonly string[];
}

interface CurrentFundsProps {
  readonly searchParams: Promise<CurrentFundsSearchParams>;
}

const createEmptyCurrent = function (): CurrentFundsModel {
  return {
    availableFundNames: [],
    summary: {
      totalTrackedBalance: 0,
      totalAssignedBalance: 0,
      totalUnassignedBalance: 0,
    },
    funds: [],
  };
};

/**
 * Component that displays the current Funds snapshot.
 */
const CurrentFunds = async function ({
  searchParams,
}: CurrentFundsProps): Promise<JSX.Element> {
  const { fundName } = await searchParams;
  const currentFundNames = normalizeRequestedFundNames(
    Array.isArray(fundName)
      ? fundName
      : typeof fundName === "string"
        ? [fundName]
        : [],
  );

  const apiClient = getApiClient();
  const current: CurrentFundsModel =
    (
      await apiClient.GET("/funds/current", {
        params: {
          query: {
            ...(shouldPersistFundNames(currentFundNames)
              ? { FundName: [...currentFundNames] }
              : {}),
          },
        },
      })
    ).data ?? createEmptyCurrent();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <CurrentFundsFilter availableFundNames={current.availableFundNames} />
      </Stack>
      <CurrentFundsSummaryCard current={current} />
      <CurrentFundsList current={current} />
    </Stack>
  );
};

export type { CurrentFundsSearchParams };
export default CurrentFunds;
