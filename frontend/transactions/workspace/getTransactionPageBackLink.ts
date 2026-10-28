import {
  type ReturnLinkTarget,
  getReturnLinkTarget,
} from "@/framework/view/ReturnLink";

/**
 * Gets the back link for a transaction page: the page that sent the user here when a return link supports it, or else
 * the transaction workspace.
 */
const getTransactionPageBackLink = function (
  returnUrl: string | undefined,
  workspaceUrl: string,
): ReturnLinkTarget {
  return (
    getReturnLinkTarget(returnUrl) ?? {
      href: workspaceUrl,
      label: "Back to Transactions",
    }
  );
};

export default getTransactionPageBackLink;
