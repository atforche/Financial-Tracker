"use client";

import { Button } from "@mui/material";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import SearchBar from "@/framework/listframe/SearchBar";
import propertyName from "@/framework/data/propertyName";
import routes from "@/funds/routes";

/**
 * Props for the FundWorkspaceFilter component.
 */
interface FundWorkspaceFilterProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Renders the filter card for the Fund workspace with header, search bar, and primary action.
 */
const FundWorkspaceFilter = function ({
  isInOnboardingMode,
}: FundWorkspaceFilterProps): JSX.Element {
  const addFundHref = isInOnboardingMode
    ? routes.workspaceOnboard({})
    : routes.workspaceCreate({});

  return (
    <PageFilterFrame
      title="Fund Workspace"
      actions={
        <Button variant="contained" href={addFundHref}>
          {isInOnboardingMode ? "Onboard Fund" : "Create Fund"}
        </Button>
      }
    >
      <SearchBar
        searchParamName={propertyName<FundWorkspaceSearchParams>("search")}
      />
    </PageFilterFrame>
  );
};

export default FundWorkspaceFilter;
