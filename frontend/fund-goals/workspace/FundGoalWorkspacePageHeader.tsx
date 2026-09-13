import { Button, Stack, Typography } from "@mui/material";
import PeriodNavigation, {
  type PeriodLink,
} from "@/framework/view/PeriodNavigation";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Props for the FundGoalWorkspacePageHeader component.
 */
interface FundGoalWorkspacePageHeaderProps {
  readonly backHref: string;
  readonly previousPeriod: PeriodLink | null;
  readonly nextPeriod: PeriodLink | null;
}

/**
 * Displays the title and return navigation for a Fund Goal workspace page.
 */
const FundGoalWorkspacePageHeader = function ({
  backHref,
  previousPeriod,
  nextPeriod,
}: FundGoalWorkspacePageHeaderProps): JSX.Element {
  return (
    <Stack spacing={2.5}>
      <Link
        href={backHref}
        style={{ alignSelf: "flex-start", textDecoration: "none" }}
      >
        <Button component="span" startIcon={<ArrowBack />}>
          Back to Workspace
        </Button>
      </Link>
      <Stack
        direction={{ xs: "column", sm: "row" }}
        alignItems={{ xs: "flex-start", sm: "center" }}
        justifyContent="space-between"
        gap={2}
      >
        <Typography variant="h4">Fund Goal Details</Typography>
        <PeriodNavigation
          previousPeriod={previousPeriod}
          nextPeriod={nextPeriod}
        />
      </Stack>
    </Stack>
  );
};

export default FundGoalWorkspacePageHeader;
