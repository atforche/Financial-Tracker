import { Box, Stack } from "@mui/material";
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
  page?: number;
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

  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Years: Array.isArray(years) ? years : typeof years !== "undefined" ? [years] : [],
        Months: Array.isArray(months) ? months : typeof months !== "undefined" ? [months] : [],
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });

  const { data: accountingPeriods } = await accountingPeriodsPromise;
  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  const defaultAccountingPeriodId = accountingPeriods.items[0]?.id ?? null;
  const isInOnboardingMode = accountingPeriods.items.length === 0;
  const selectedAccountingPeriod =
    accountingPeriods.items.find(
      (accountingPeriod) => accountingPeriod.id === selectedAccountingPeriodId,
    ) ?? null;

  if (
    typeof selectedAccountingPeriodId === "string" &&
    selectedAccountingPeriod === null
  ) {
    redirect(routes.workspace({
      years: Array.isArray(years) ? years : typeof years !== "undefined" ? [years] : [],
      months: Array.isArray(months) ? months : typeof months !== "undefined" ? [months] : [],
      ...(typeof sort !== "undefined" ? { sort } : {}),
      ...(typeof page !== "undefined" ? { page } : {}),
      selectedAccountingPeriodId: "",
      ...(typeof action !== "undefined" ? { action } : {}),
    }));
  }

  const redirectSearchParams: AccountingPeriodWorkspaceSearchParams = {
    ...(typeof resolvedSearchParams.startAccountingPeriodId !== "undefined"
      ? {
          startAccountingPeriodId: resolvedSearchParams.startAccountingPeriodId,
        }
      : {}),
    ...(typeof resolvedSearchParams.endAccountingPeriodId !== "undefined"
      ? { endAccountingPeriodId: resolvedSearchParams.endAccountingPeriodId }
      : {}),
    ...(currentPage > 1 ? { page: currentPage } : {}),
  };
  const createRedirectUrl = routes.workspace(redirectSearchParams);
  const selectedRedirectSearchParams: AccountingPeriodWorkspaceSearchParams =
    selectedAccountingPeriod === null
      ? redirectSearchParams
      : {
          ...redirectSearchParams,
          startAccountingPeriodId: selectedAccountingPeriod.id,
          endAccountingPeriodId: selectedAccountingPeriod.id,
        };
  const selectedRedirectUrl = routes.workspace(selectedRedirectSearchParams);

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountingPeriodWorkspaceFilter
          accountingPeriods={accountingPeriods.items}
          disabled={defaultAccountingPeriodId === null}
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
          accountingPeriods={accountingPeriods.items}
          defaultAccountingPeriodId={defaultAccountingPeriodId}
        />
        <AccountingPeriodWorkspaceActions
          isInOnboardingMode={isInOnboardingMode}
          selectedAccountingPeriod={selectedAccountingPeriod}
          requestedAction={resolvedSearchParams.action ?? null}
          createRedirectUrl={createRedirectUrl}
          closeRedirectUrl={selectedRedirectUrl}
          reopenRedirectUrl={selectedRedirectUrl}
          deleteRedirectUrl={selectedRedirectUrl}
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
