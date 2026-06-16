import type { JSX } from "react";
import OverviewView from "@/overview/OverviewView";

/**
 * Component that displays the home page, which currently consists solely of the Overview view.
 */
const HomePage = function (): JSX.Element {
  return <OverviewView />;
};

export const dynamic = "force-dynamic";
export default HomePage;
