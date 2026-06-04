import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import AccountingPeriodWorkspaceActions from "@/accounting-periods/workspace/AccountingPeriodWorkspaceActions";
import AccountingPeriodWorkspaceFilter from "@/accounting-periods/workspace/AccountingPeriodWorkspaceFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";

type AccountingPeriodWorkspaceAction = "create" | "close" | "reopen" | "delete";

/**
 * Search parameters supported by the Accounting Period workspace.
 */
interface AccountingPeriodWorkspaceSearchParams {
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  page?: number | string;
  action?: AccountingPeriodWorkspaceAction;
}

/**
 * Props for the AccountingPeriodWorkspace component.
 */
interface AccountingPeriodWorkspaceProps {
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

const parsePageNumber = function (
  page: AccountingPeriodWorkspaceSearchParams["page"],
): number {
  const pageNumber =
    typeof page === "number" ? page : Number.parseInt(page ?? "1", 10);
  return Number.isFinite(pageNumber) && pageNumber > 0 ? pageNumber : 1;
};

const isValidAction = function (
  action: string | null,
): action is AccountingPeriodWorkspaceAction {
  return (
    action === "create" ||
    action === "close" ||
    action === "reopen" ||
    action === "delete"
  );
};

const normalizeSearchParams = function (
  searchParams: AccountingPeriodWorkspaceSearchParams,
): AccountingPeriodWorkspaceSearchParams {
  return {
    ...(typeof searchParams.startAccountingPeriodId === "string" &&
    searchParams.startAccountingPeriodId !== ""
      ? { startAccountingPeriodId: searchParams.startAccountingPeriodId }
      : {}),
    ...(typeof searchParams.endAccountingPeriodId === "string" &&
    searchParams.endAccountingPeriodId !== ""
      ? { endAccountingPeriodId: searchParams.endAccountingPeriodId }
      : {}),
    ...(parsePageNumber(searchParams.page) > 1
      ? { page: parsePageNumber(searchParams.page) }
      : {}),
    ...(isValidAction(searchParams.action ?? null)
      ? { action: searchParams.action }
      : {}),
  };
};

/**
 * Displays the accounting period workspace.
 */
const AccountingPeriodWorkspace = async function ({
  searchParams,
}: AccountingPeriodWorkspaceProps): Promise<JSX.Element> {
  const resolvedSearchParams = normalizeSearchParams(await searchParams);
  const currentPage = parsePageNumber(resolvedSearchParams.page);
  const apiClient = getApiClient();

  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );

  const [{ data: accountingPeriods }, { data: openAccountingPeriods }] =
    await Promise.all([accountingPeriodsPromise, openAccountingPeriodsPromise]);

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }

  const defaultAccountingPeriodId = accountingPeriods.items[0]?.id ?? null;
  const isInOnboardingMode = openAccountingPeriods.length === 0;
  const selectedAccountingPeriodId =
    resolvedSearchParams.endAccountingPeriodId ??
    resolvedSearchParams.startAccountingPeriodId ??
    defaultAccountingPeriodId;
  const selectedAccountingPeriod =
    typeof selectedAccountingPeriodId === "string"
      ? (accountingPeriods.items.find(
          (accountingPeriod) =>
            accountingPeriod.id === selectedAccountingPeriodId,
        ) ?? null)
      : null;

  if (
    selectedAccountingPeriodId !== null &&
    selectedAccountingPeriod === null
  ) {
    const nextSearchParams: AccountingPeriodWorkspaceSearchParams = {
      ...(currentPage > 1 ? { page: currentPage } : {}),
    };
    if (defaultAccountingPeriodId !== null) {
      nextSearchParams.startAccountingPeriodId = defaultAccountingPeriodId;
      nextSearchParams.endAccountingPeriodId = defaultAccountingPeriodId;
    }
    redirect(routes.workspace(nextSearchParams));
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
          defaultAccountingPeriodId={defaultAccountingPeriodId}
          disabled={defaultAccountingPeriodId === null}
        />
      </Stack>
      <AccountingPeriodWorkspaceActions
        isInOnboardingMode={isInOnboardingMode}
        selectedAccountingPeriod={selectedAccountingPeriod}
        requestedAction={resolvedSearchParams.action ?? null}
        createRedirectUrl={createRedirectUrl}
        closeRedirectUrl={selectedRedirectUrl}
        reopenRedirectUrl={selectedRedirectUrl}
        deleteRedirectUrl={selectedRedirectUrl}
      />
    </Stack>
  );
};

export type {
  AccountingPeriodWorkspaceAction,
  AccountingPeriodWorkspaceSearchParams,
};
export default AccountingPeriodWorkspace;
