import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import AccountingPeriodWorkspaceActions from "@/accounting-periods/workspace/AccountingPeriodWorkspaceActions";
import AccountingPeriodWorkspaceFilter from "@/accounting-periods/workspace/AccountingPeriodWorkspaceFilter";
import AccountingPeriodWorkspaceListFrame from "@/accounting-periods/workspace/AccountingPeriodWorkspaceListFrame";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type AccountingPeriodWorkspaceAction = "create" | "close" | "reopen" | "delete";

/**
 * Search parameters supported by the Accounting Period workspace.
 */
interface AccountingPeriodWorkspaceSearchParams {
  years?: number | number[];
  months?: number | number[];
  sort?: AccountingPeriodSortOrder;
  page?: number | string | null;
  selectedAccountingPeriodId?: string;
  action?: AccountingPeriodWorkspaceAction;
}

/**
 * Props for the AccountingPeriodWorkspace component.
 */
interface AccountingPeriodWorkspaceProps {
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays the accounting period workspace.
 */
const AccountingPeriodWorkspace = async function ({
  searchParams,
}: AccountingPeriodWorkspaceProps): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const { years, months, sort, page, selectedAccountingPeriodId, action } =
    await searchParams;
  const currentPage = normalizePageValue(page);

  const firstAccountingPeriodPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: AccountingPeriodSortOrder.Date,
        Limit: 1,
      },
    },
  });
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        ...(Array.isArray(years)
          ? { Years: years }
          : typeof years !== "undefined"
            ? { Years: [years] }
            : {}),
        ...(Array.isArray(months)
          ? { Months: months }
          : typeof months !== "undefined"
            ? { Months: [months] }
            : {}),
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });

  const [
    { data: firstAccountingPeriodResponse },
    { data: accountingPeriodsResponse },
  ] = await Promise.all([
    firstAccountingPeriodPromise,
    accountingPeriodsPromise,
  ]);

  const firstAccountingPeriod = firstAccountingPeriodResponse ?? {
    items: [],
    totalCount: 0,
  };
  const accountingPeriods = accountingPeriodsResponse ?? {
    items: [],
    totalCount: 0,
  };

  const selectedAccountingPeriod =
    accountingPeriods.items.find(
      (accountingPeriod) => accountingPeriod.id === selectedAccountingPeriodId,
    ) ?? null;
  const isInOnboardingMode = firstAccountingPeriod.items.length === 0;

  if (
    typeof selectedAccountingPeriodId === "string" &&
    selectedAccountingPeriod === null
  ) {
    redirect(
      routes.workspace({
        years: Array.isArray(years)
          ? years
          : typeof years !== "undefined"
            ? [years]
            : [],
        months: Array.isArray(months)
          ? months
          : typeof months !== "undefined"
            ? [months]
            : [],
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        ...(typeof action !== "undefined" ? { action } : {}),
      }),
    );
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountingPeriodWorkspaceFilter
          firstAccountingPeriod={firstAccountingPeriod.items[0] ?? null}
        />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 600px), 1fr))",
        }}
      >
        <AccountingPeriodWorkspaceListFrame
          data={accountingPeriods.items}
          totalCount={accountingPeriods.totalCount}
          selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
        />
        <AccountingPeriodWorkspaceActions
          isInOnboardingMode={isInOnboardingMode}
          selectedAccountingPeriod={selectedAccountingPeriod}
          requestedAction={action ?? null}
        />
      </Box>
    </Stack>
  );
};

export type {
  AccountingPeriodWorkspaceAction,
  AccountingPeriodWorkspaceSearchParams,
};
export default AccountingPeriodWorkspace;
