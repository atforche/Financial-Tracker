import ArrowBack from "@mui/icons-material/ArrowBack";
import { Button } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";

interface TrendsBackLinkProps {
  readonly returnUrl?: string | undefined;
  readonly workspace: "funds" | "accounts" | "fund-goals";
  readonly label: string;
}

/** Shows a return link only for a detail page in the expected workspace. */
const TrendsBackLink = function ({
  returnUrl,
  workspace,
  label,
}: TrendsBackLinkProps): JSX.Element | null {
  if (
    typeof returnUrl !== "string" ||
    !returnUrl.startsWith(`/${workspace}/workspace/`) ||
    !/^\/[a-z-]+\/workspace\/[^/?#]+(?:\?[^#]*)?$/u.test(returnUrl)
  ) {
    return null;
  }

  return (
    <Link
      href={returnUrl}
      style={{ alignSelf: "flex-start", textDecoration: "none" }}
    >
      <Button component="span" startIcon={<ArrowBack />}>
        {label}
      </Button>
    </Link>
  );
};

export default TrendsBackLink;
