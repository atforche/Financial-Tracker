import { Breadcrumbs as MuiBreadcrumbs, Typography } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Interface representing a breadcrumb item in the application's navigation.
 */
interface Breadcrumb {
  readonly label: string;
  readonly href: string;
}

/**
 * Component that renders a breadcrumb navigation based on the provided breadcrumbs.
 */
const Breadcrumbs = function ({
  breadcrumbs,
}: {
  readonly breadcrumbs: Breadcrumb[];
}): JSX.Element {
  return (
    <MuiBreadcrumbs aria-label="breadcrumb">
      {breadcrumbs.map((breadcrumb, index) =>
        index === breadcrumbs.length - 1 ? (
          <Typography variant="h6" key={index} color="text.primary">
            {breadcrumb.label}
          </Typography>
        ) : (
          <Link
            key={index}
            href={breadcrumb.href}
            style={{ textDecoration: "none", color: "inherit" }}
          >
            <Typography variant="h6" color="text.primary">
              {breadcrumb.label}
            </Typography>
          </Link>
        ),
      )}
    </MuiBreadcrumbs>
  );
};

export default Breadcrumbs;
