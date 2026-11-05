import { Box, Chip, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodDetailActions from "@/accounting-periods/workspace/AccountingPeriodDetailActions";
import type { AccountingPeriodSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspaceDetailPage";
import type { JSX } from "react";
import PeriodNavigation from "@/framework/view/PeriodNavigation";
import ReturnLink from "@/framework/view/ReturnLink";
import type { components } from "@/framework/data/api";
import routes from "@/accounting-periods/routes";

/**
 * The widest period name, which reserves the title's width so the arrows beside it don't move between periods.
 */
const longestPeriodName = "September 2026";

/**
 * Stacks the title and its width reservation in one cell, at a size that fits beside the arrows on a phone.
 */
const titleSx = {
  gridArea: "1 / 1",
  fontSize: { xs: "1.25rem", sm: "2.125rem" },
};

/**
 * Props for the AccountingPeriodHeader component.
 */
interface AccountingPeriodHeaderProps {
  readonly detail: components["schemas"]["AccountingPeriodDetailModel"];
  readonly searchParams: AccountingPeriodSearchParams;
  readonly currentUrl: string;
}

/**
 * Displays an accounting period's name and status, links to its neighbors, and its lifecycle actions.
 */
const AccountingPeriodHeader = function ({
  detail,
  searchParams,
  currentUrl,
}: AccountingPeriodHeaderProps): JSX.Element {
  const period = detail.accountingPeriod;
  const workspaceUrl = routes.workspace({});
  // Neighbors keep the selected tab and filters, and still return to the page the user came from
  const getNeighborLink = function (
    neighbor: AccountingPeriod | null | undefined,
  ): { name: string; href: ReturnType<typeof routes.workspaceDetail> } | null {
    return neighbor === null || typeof neighbor === "undefined"
      ? null
      : {
          name: neighbor.name,
          href: routes.workspaceDetail(neighbor.id, searchParams),
        };
  };

  return (
    <Stack spacing={2}>
      <ReturnLink
        returnUrl={searchParams.returnUrl}
        fallback={{ href: workspaceUrl, label: "Back to Accounting Periods" }}
      />
      <Stack
        direction="row"
        spacing={1.5}
        sx={{ alignItems: "center", justifyContent: "space-between" }}
      >
        <Stack
          direction="row"
          spacing={1.5}
          sx={{ alignItems: "center", minWidth: 0 }}
        >
          <PeriodNavigation
            previousPeriod={getNeighborLink(detail.previousPeriod)}
            nextPeriod={getNeighborLink(detail.nextPeriod)}
          >
            <Box
              sx={{
                display: "grid",
                minWidth: 0,
                overflow: "hidden",
                textAlign: "center",
              }}
            >
              <Typography variant="h4" noWrap sx={{ ...titleSx }}>
                {period.name}
              </Typography>
              <Typography
                aria-hidden
                component="span"
                variant="h4"
                sx={{ ...titleSx, visibility: "hidden", whiteSpace: "nowrap" }}
              >
                {longestPeriodName}
              </Typography>
            </Box>
          </PeriodNavigation>
          <Chip
            label={period.isOpen ? "Open" : "Closed"}
            color={period.isOpen ? "success" : "default"}
            size="small"
            variant="outlined"
          />
        </Stack>
        <AccountingPeriodDetailActions
          accountingPeriod={period}
          redirectUrl={currentUrl}
          deleteRedirectUrl={workspaceUrl}
        />
      </Stack>
    </Stack>
  );
};

export default AccountingPeriodHeader;
