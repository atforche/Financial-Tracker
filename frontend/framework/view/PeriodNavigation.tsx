import { Button, Stack } from "@mui/material";
import ChevronLeft from "@mui/icons-material/ChevronLeft";
import ChevronRight from "@mui/icons-material/ChevronRight";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";

interface PeriodLink {
  readonly name: string;
  readonly href: Route;
}

interface PeriodNavigationProps {
  readonly previousPeriod: PeriodLink | null;
  readonly nextPeriod: PeriodLink | null;
}

/** Links to adjacent available accounting periods. */
const PeriodNavigation = function ({
  previousPeriod,
  nextPeriod,
}: PeriodNavigationProps): JSX.Element {
  return (
    <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
      {previousPeriod === null ? null : (
        <Link
          href={previousPeriod.href}
          title={previousPeriod.name}
          style={{ textDecoration: "none" }}
        >
          <Button
            component="span"
            variant="outlined"
            startIcon={<ChevronLeft />}
          >
            Previous period
          </Button>
        </Link>
      )}
      {nextPeriod === null ? null : (
        <Link
          href={nextPeriod.href}
          title={nextPeriod.name}
          style={{ textDecoration: "none" }}
        >
          <Button
            component="span"
            variant="outlined"
            endIcon={<ChevronRight />}
          >
            Next period
          </Button>
        </Link>
      )}
    </Stack>
  );
};

export default PeriodNavigation;
export type { PeriodLink };
