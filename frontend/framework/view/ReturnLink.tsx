import ArrowBack from "@mui/icons-material/ArrowBack";
import { Button } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";

/**
 * Labels for the pages a return link can lead back to, matched against the return path.
 */
const returnLabels: readonly (readonly [RegExp, string])[] = [
  [/^\/(?:\?|$)/u, "Back to Overview"],
  [/^\/accounting-periods\/trends(?:\?|$)/u, "Back to Trends"],
  [/^\/locations\/workspace\/[^/?#]+(?:\?|$)/u, "Back to Location"],
  [/^\/accounts\/workspace\/[^/?#]+(?:\?|$)/u, "Back to Account Details"],
  [/^\/funds\/workspace\/[^/?#]+(?:\?|$)/u, "Back to Fund Details"],
  [
    /^\/account-goals\/workspace\/[^/?#]+(?:\?|$)/u,
    "Back to Account Goal Details",
  ],
  [/^\/fund-goals\/workspace\/[^/?#]+(?:\?|$)/u, "Back to Fund Goal Details"],
];

/**
 * Gets the label for a link back to the provided URL, or null when the URL is not a page within the app that a return
 * link supports.
 */
const getReturnLabel = function (
  returnUrl: string | readonly string[] | undefined,
): string | null {
  if (
    typeof returnUrl !== "string" ||
    !returnUrl.startsWith("/") ||
    returnUrl.startsWith("//") ||
    returnUrl.includes("\\")
  ) {
    return null;
  }
  return returnLabels.find(([pattern]) => pattern.test(returnUrl))?.[1] ?? null;
};

/**
 * Props for the ReturnLink component.
 */
interface ReturnLinkProps {
  readonly returnUrl: string | readonly string[] | undefined;
  readonly fallback?: { readonly href: Route; readonly label: string };
}

/**
 * Shows a link back to the page that sent the user here, when that page is one a return link supports, or else the
 * fallback link when one is provided.
 */
const ReturnLink = function ({
  returnUrl,
  fallback,
}: ReturnLinkProps): JSX.Element | null {
  const resolvedLabel = getReturnLabel(returnUrl);
  const link =
    resolvedLabel === null || typeof returnUrl !== "string"
      ? (fallback ?? null)
      : { href: returnUrl, label: resolvedLabel };
  if (link === null) {
    return null;
  }

  return (
    <Link
      href={link.href}
      style={{ alignSelf: "flex-start", textDecoration: "none" }}
    >
      <Button component="span" startIcon={<ArrowBack />}>
        {link.label}
      </Button>
    </Link>
  );
};

export default ReturnLink;
