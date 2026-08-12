import ExpectedIncomeSourcePage, {
  type ExpectedIncomeSourcePageRouteProps,
} from "@/accounting-periods/workspace/ExpectedIncomeSourcePage";
import type { JSX } from "react";

/** Displays one expected-income source. */
const ViewExpectedIncomeSourcePage = function (
  props: ExpectedIncomeSourcePageRouteProps,
): JSX.Element {
  return <ExpectedIncomeSourcePage {...props} mode="view" />;
};

export const dynamic = "force-dynamic";
export default ViewExpectedIncomeSourcePage;
