"use client";

import ActionIconButton from "@/framework/view/ActionIconButton";
import { Button } from "@mui/material";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import SearchBar from "@/framework/view/SearchBar";
import WorkspaceHeader from "@/framework/view/WorkspaceHeader";
import propertyName from "@/framework/data/propertyName";
import routes from "@/funds/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the FundWorkspaceFilter component.
 */
interface FundWorkspaceFilterProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Renders the Funds workspace header with its search and primary action.
 */
const FundWorkspaceFilter = function ({
  isInOnboardingMode,
}: FundWorkspaceFilterProps): JSX.Element {
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const searchParamName = propertyName<FundWorkspaceSearchParams>("search");
  const hasSearch = (searchParams.get(searchParamName) ?? "").trim() !== "";
  const addFundHref = isInOnboardingMode
    ? routes.workspaceOnboard({})
    : routes.workspaceCreate({});

  return (
    <WorkspaceHeader
      title="Funds"
      action={
        canWrite ? (
          <ActionIconButton
            action="add"
            label={isInOnboardingMode ? "Onboard fund" : "Create fund"}
            href={addFundHref}
          />
        ) : null
      }
    >
      <SearchBar searchParamName={searchParamName} />
      <Button
        disabled={!hasSearch}
        onClick={() => {
          updateParams((params) => {
            params.delete(searchParamName);
          });
        }}
      >
        Reset Filters
      </Button>
    </WorkspaceHeader>
  );
};

export default FundWorkspaceFilter;
