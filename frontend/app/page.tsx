import type { JSX } from "react";
import OverviewView from "@/overview/OverviewView";

interface HomePageProps {
  readonly searchParams: Promise<{ page?: string | string[] }>;
}

/**
 * Component that displays the home page, which currently consists solely of the Overview view.
 */
const HomePage = function ({ searchParams }: HomePageProps): JSX.Element {
  return <OverviewView searchParams={searchParams} />;
};

export const dynamic = "force-dynamic";
export default HomePage;
