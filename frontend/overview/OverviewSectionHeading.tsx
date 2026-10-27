import { Button, Stack, Typography } from "@mui/material";
import ArrowForward from "@mui/icons-material/ArrowForward";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";

/**
 * Props for the OverviewSectionHeading component.
 */
interface OverviewSectionHeadingProps {
  readonly title: string;
  readonly trendsHref: Route;
}

/**
 * Displays an overview section title with a link to that section's trends page.
 */
const OverviewSectionHeading = function ({
  title,
  trendsHref,
}: OverviewSectionHeadingProps): JSX.Element {
  return (
    <Stack direction="row" alignItems="center" justifyContent="space-between">
      <Typography variant="h5">{title}</Typography>
      <Link href={trendsHref} style={{ textDecoration: "none" }}>
        <Button component="span" endIcon={<ArrowForward />}>
          View trends
        </Button>
      </Link>
    </Stack>
  );
};

export default OverviewSectionHeading;
