"use client";

import {
  type FundWithBalanceRange,
  FundWithBalanceRangeSort,
} from "@/funds/types";
import PagedTable, { type PagedTableColumn } from "@/framework/view/PagedTable";
import {
  clearFundTrendsFilters,
  fundTrendsParamNames,
  hasActiveFundTrendsFilters,
} from "@/funds/trends/helpers";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type { DetailViewParams } from "@/balance-events/getDetailViewParams";
import type { JSX } from "react";
import SignedChange from "@/framework/view/SignedChange";
import routes from "@/funds/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the FundTrendsListFrame component.
 */
interface FundTrendsListFrameProps {
  readonly data: readonly FundWithBalanceRange[];
  readonly totalCount: number;
  readonly isInOnboardingMode: boolean;

  /**
   * The trends range and return link, so each fund opens over the same range.
   */
  readonly detailParams: DetailViewParams;
}

const columns: readonly PagedTableColumn<FundWithBalanceRange>[] = [
  {
    name: "name",
    header: "Name",
    getValue: (fund) => fund.name,
    width: "minmax(0, 2fr)",
    sort: [
      FundWithBalanceRangeSort.Name,
      FundWithBalanceRangeSort.NameDescending,
    ],
  },
  {
    name: "startingBalance",
    header: "Starting Balance",
    getValue: (fund) => formatCurrency(fund.startingBalance),
    width: "minmax(0, 1fr)",
    alignment: "right",
    hideOnMobile: true,
    sort: [
      FundWithBalanceRangeSort.StartingBalance,
      FundWithBalanceRangeSort.StartingBalanceDescending,
    ],
  },
  {
    name: "endingBalance",
    header: "Ending Balance",
    getValue: (fund) => formatCurrency(fund.endingBalance),
    width: "minmax(0, 1fr)",
    // Phones size amounts to fit, so the name gives up the space instead of digits
    mobileWidth: "max-content",
    alignment: "right",
    sort: [
      FundWithBalanceRangeSort.EndingBalance,
      FundWithBalanceRangeSort.EndingBalanceDescending,
    ],
  },
  {
    name: "change",
    header: "Net Change",
    getValue: (fund) => (
      <SignedChange
        amount={getCurrencyDifference(fund.endingBalance, fund.startingBalance)}
        isIncreaseGood
      />
    ),
    width: "minmax(0, 1fr)",
    // Phones size amounts to fit, so the name gives up the space instead of digits
    mobileWidth: "max-content",
    alignment: "right",
    sort: [
      FundWithBalanceRangeSort.NetChange,
      FundWithBalanceRangeSort.NetChangeDescending,
    ],
  },
];

/**
 * Presents the paged funds for the Funds trends, each linking to the fund over the same range.
 */
const FundTrendsListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
  detailParams,
}: FundTrendsListFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();
  const router = useRouter();
  const updateParams = useSearchParamUpdater([fundTrendsParamNames.page]);

  const emptyState = hasActiveFundTrendsFilters(searchParams)
    ? {
        title: "No Funds Match This Trends Filter",
        description:
          "Try a different fund name or date range to widen the trends scope.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                clearFundTrendsFilters(params);
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }
    : {
        title: "No Funds Have Been Added",
        description: isInOnboardingMode
          ? "Onboard a new fund to start tracking balances."
          : "Create a new fund to start tracking balances.",
        action: !canWrite ? null : (
          <Button
            variant="contained"
            onClick={() => {
              router.push(
                isInOnboardingMode
                  ? routes.workspaceOnboard({})
                  : routes.workspaceCreate({}),
              );
            }}
          >
            {isInOnboardingMode ? "Onboard fund" : "Create fund"}
          </Button>
        ),
      };

  return (
    <PagedTable
      title="Funds"
      columns={columns}
      data={data}
      getKey={(fund) => fund.id}
      getHref={(fund) => routes.workspaceDetail(fund.id, detailParams)}
      totalCount={totalCount}
      defaultSort={FundWithBalanceRangeSort.Name}
      sortParamName={fundTrendsParamNames.sort}
      pageParamName={fundTrendsParamNames.page}
      emptyState={emptyState}
    />
  );
};

export default FundTrendsListFrame;
