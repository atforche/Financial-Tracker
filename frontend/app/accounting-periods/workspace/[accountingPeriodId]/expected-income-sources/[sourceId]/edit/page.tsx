import ExpectedIncomeSourcePage from "@/accounting-periods/workspace/ExpectedIncomeSourcePage";
import type { JSX } from "react";

/**
 * Props for the Page component, which displays an expected-income source page.
 */
interface PageProps {
  readonly params: Promise<{ accountingPeriodId: string; sourceId: string }>;
  readonly searchParams: Promise<{ returnUrl?: string }>;
}

/**
 * Displays an expected-income source page in edit mode.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { accountingPeriodId, sourceId } = await params;
  const { returnUrl } = await searchParams;
  return (
    <ExpectedIncomeSourcePage
      accountingPeriodId={accountingPeriodId}
      sourceId={sourceId}
      mode="change"
      returnUrl={returnUrl}
    />
  );
};

export default Page;
export const dynamic = "force-dynamic";
