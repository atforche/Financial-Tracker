import type {
  PostTransactionViewParams,
  PostTransactionViewSearchParams,
} from "@/transactions/PostTransactionView";
import type {
  TransactionViewParams,
  TransactionViewSearchParams,
} from "@/transactions/TransactionView";
import type {
  UnpostTransactionViewParams,
  UnpostTransactionViewSearchParams,
} from "@/transactions/UnpostTransactionView";
import type { CreateTransactionViewSearchParams } from "@/transactions/CreateTransactionView";
import type { DeleteTransactionViewParams } from "@/transactions/DeleteTransactionView";
import type { Route } from "next";
import type { TransactionsViewSearchParams } from "@/transactions/TransactionsView";
import type { UpdateTransactionViewParams } from "@/transactions/UpdateTransactionView";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to transactions.
 */
const routes = {
  index: (searchParams: TransactionsViewSearchParams): Route =>
    `/transactions?${objectToSearchParams(searchParams).toString()}`,
  create: (searchParams: CreateTransactionViewSearchParams): Route =>
    `/transactions/create?${objectToSearchParams(searchParams).toString()}`,
  detail: (
    params: TransactionViewParams,
    searchParams: TransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}?${objectToSearchParams(searchParams).toString()}`,
  update: (params: UpdateTransactionViewParams): Route =>
    `/transactions/${params.id}/update`,
  post: (
    params: PostTransactionViewParams,
    searchParams: PostTransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}/post?${objectToSearchParams(searchParams).toString()}`,
  unpost: (
    params: UnpostTransactionViewParams,
    searchParams: UnpostTransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}/unpost?${objectToSearchParams(searchParams).toString()}`,
  delete: (params: DeleteTransactionViewParams): Route =>
    `/transactions/${params.id}/delete`,
};

export default routes;
