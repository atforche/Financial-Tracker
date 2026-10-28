import AccountingPeriodViewTabs, {
  type AccountingPeriodView,
} from "@/accounting-periods/workspace/AccountingPeriodViewTabs";
import { Box, Stack, Typography } from "@mui/material";
import AccountingPeriodDetailActions from "@/accounting-periods/workspace/AccountingPeriodDetailActions";
import AccountingPeriodSummaryFrame from "@/accounting-periods/workspace/AccountingPeriodSummaryFrame";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { JSX } from "react";
import PeriodNavigation from "@/framework/view/PeriodNavigation";
import ReturnLink from "@/framework/view/ReturnLink";
import type { Route } from "next";

/**
 * A link to an adjacent accounting period.
 */
interface AdjacentPeriodLink {
  readonly name: string;
  readonly href: Route;
}

/**
 * Props for the AccountingPeriodWorkspaceHeader component.
 */
interface AccountingPeriodWorkspaceHeaderProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly workspaceUrl: Route;
  readonly previousPeriod: AdjacentPeriodLink | null;
  readonly nextPeriod: AdjacentPeriodLink | null;
  readonly activeView: AccountingPeriodView;
  readonly cashFlowHref: Route;
  readonly planHref: Route;
  readonly redirectUrl: string;
  readonly returnUrl: string | undefined;
}

/**
 * Displays the heading, summary, actions, and view tabs shared by every view of an accounting period.
 */
const AccountingPeriodWorkspaceHeader = function ({
  accountingPeriod,
  workspaceUrl,
  previousPeriod,
  nextPeriod,
  activeView,
  cashFlowHref,
  planHref,
  redirectUrl,
  returnUrl,
}: AccountingPeriodWorkspaceHeaderProps): JSX.Element {
  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Stack spacing={2.5}>
        <ReturnLink
          returnUrl={returnUrl}
          fallback={{ href: workspaceUrl, label: "Back to Workspace" }}
        />
        <Stack
          direction={{ xs: "column", sm: "row" }}
          alignItems={{ xs: "flex-start", sm: "center" }}
          justifyContent="space-between"
          gap={2}
        >
          <Typography variant="h4">{accountingPeriod.name}</Typography>
          <PeriodNavigation
            previousPeriod={previousPeriod}
            nextPeriod={nextPeriod}
          />
        </Stack>
        <AccountingPeriodSummaryFrame
          accountingPeriod={accountingPeriod}
          headerContent={
            <AccountingPeriodDetailActions
              accountingPeriod={accountingPeriod}
              redirectUrl={redirectUrl}
              deleteRedirectUrl={workspaceUrl}
            />
          }
        />
        <AccountingPeriodViewTabs
          activeView={activeView}
          cashFlowHref={cashFlowHref}
          planHref={planHref}
        />
      </Stack>
    </Box>
  );
};

export default AccountingPeriodWorkspaceHeader;
