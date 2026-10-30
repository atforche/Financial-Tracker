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
  [/^\/transactions\/workspace(?:\?|$)/u, "Back to Transactions"],
  [
    /^\/(?:accounting-periods|accounts|funds|locations)\/trends(?:\?|$)/u,
    "Back to Trends",
  ],
  [
    /^\/accounting-periods\/workspace\/[^/?#]+\/expected-income-sources\/[^/?#]+(?:\?|$)/u,
    "Back to Expected Income Source",
  ],
  [
    /^\/accounting-periods\/workspace\/[^/?#]+(?:\?|$)/u,
    "Back to Accounting Period",
  ],
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
 * Checks whether a return URL is a path within the app, so it can't send the user to another site.
 */
const isInternalUrl = function (
  returnUrl: string | readonly string[] | null | undefined,
): returnUrl is string {
  return (
    typeof returnUrl === "string" &&
    returnUrl.startsWith("/") &&
    !returnUrl.startsWith("//") &&
    !returnUrl.includes("\\")
  );
};

/**
 * A link back to a page, with its label.
 */
interface ReturnLinkTarget {
  readonly href: string;
  readonly label: string;
}

/**
 * Gets the link back to the provided URL, or null when the URL is not a page within the app that a return link
 * supports.
 */
const getReturnLinkTarget = function (
  returnUrl: string | readonly string[] | null | undefined,
): ReturnLinkTarget | null {
  if (!isInternalUrl(returnUrl)) {
    return null;
  }
  const label = returnLabels.find(([pattern]) => pattern.test(returnUrl))?.[1];
  return label === undefined ? null : { href: returnUrl, label };
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
  const link = getReturnLinkTarget(returnUrl) ?? fallback ?? null;
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

export { getReturnLinkTarget };
export type { ReturnLinkTarget };
export default ReturnLink;
