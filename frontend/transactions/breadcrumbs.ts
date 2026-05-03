import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Fund } from "@/funds/types";
import type { Transaction } from "@/transactions/types";
import accountBreadcrumbs from "@/accounts/breadcrumbs";
import accountingPeriodBreadcrumbs from "@/accounting-periods/breadcrumbs";
import fundBreadcrumbs from "@/funds/breadcrumbs";
import routes from "@/transactions/routes";

/**
 * Breadcrumbs related to transactions.
 */
const breadcrumbs = {
  create: (
    providedAccountingPeriod: AccountingPeriod | null,
    providedAccount: Account | null,
    providedFund: Fund | null,
  ): Breadcrumb[] => {
    const currentCrumb = {
      label: "Create Transaction",
      href: routes.create({}),
    };
    if (providedAccountingPeriod !== null) {
      if (providedAccount !== null) {
        return [
          ...accountingPeriodBreadcrumbs.accountDetail(
            providedAccountingPeriod,
            providedAccount,
          ),
          currentCrumb,
        ];
      }
      if (providedFund !== null) {
        return [
          ...accountingPeriodBreadcrumbs.fundDetail(
            providedAccountingPeriod,
            providedFund,
          ),
          currentCrumb,
        ];
      }
      return [
        ...accountingPeriodBreadcrumbs.detail(providedAccountingPeriod),
        currentCrumb,
      ];
    }
    if (providedAccount !== null) {
      return [...accountBreadcrumbs.detail(providedAccount), currentCrumb];
    }
    if (providedFund !== null) {
      return [...fundBreadcrumbs.detail(providedFund), currentCrumb];
    }
    return [currentCrumb];
  },
  detail: (
    transaction: Transaction,
    providedAccountingPeriod: AccountingPeriod | null,
    providedAccount: Account | null,
    providedFund: Fund | null,
  ): Breadcrumb[] => {
    const currentCrumb = {
      label: "Transaction",
      href: routes.detail({ id: transaction.id }, {}),
    };
    if (providedAccountingPeriod !== null) {
      if (providedAccount !== null) {
        return [
          ...accountingPeriodBreadcrumbs.accountDetail(
            providedAccountingPeriod,
            providedAccount,
          ),
          currentCrumb,
        ];
      }
      if (providedFund !== null) {
        return [
          ...accountingPeriodBreadcrumbs.fundDetail(
            providedAccountingPeriod,
            providedFund,
          ),
          currentCrumb,
        ];
      }
      return [
        ...accountingPeriodBreadcrumbs.detail(providedAccountingPeriod),
        currentCrumb,
      ];
    }
    if (providedAccount !== null) {
      return [...accountBreadcrumbs.detail(providedAccount), currentCrumb];
    }
    if (providedFund !== null) {
      return [...fundBreadcrumbs.detail(providedFund), currentCrumb];
    }
    return [currentCrumb];
  },
  update: (
    transaction: Transaction,
    providedAccountingPeriod: AccountingPeriod | null,
    providedAccount: Account | null,
    providedFund: Fund | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      providedAccountingPeriod,
      providedAccount,
      providedFund,
    ),
    {
      label: "Update",
      href: routes.update({ id: transaction.id }, {}),
    },
  ],
  post: (
    transaction: Transaction,
    providedAccountingPeriod: AccountingPeriod | null,
    providedAccount: Account | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      providedAccountingPeriod,
      providedAccount,
      null,
    ),
    {
      label: "Post",
      href: routes.post({ id: transaction.id }, {}),
    },
  ],
  unpost: (
    transaction: Transaction,
    providedAccountingPeriod: AccountingPeriod | null,
    providedAccount: Account | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      providedAccountingPeriod,
      providedAccount,
      null,
    ),
    {
      label: "Unpost",
      href: routes.unpost({ id: transaction.id }, {}),
    },
  ],
  delete: (
    transaction: Transaction,
    providedAccountingPeriod: AccountingPeriod | null,
    providedAccount: Account | null,
    providedFund: Fund | null,
  ): Breadcrumb[] => [
    ...breadcrumbs.detail(
      transaction,
      providedAccountingPeriod,
      providedAccount,
      providedFund,
    ),
    {
      label: "Delete",
      href: routes.delete({ id: transaction.id }, {}),
    },
  ],
};

export default breadcrumbs;
