"use client";

import { Button, Stack } from "@mui/material";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import SearchBar from "@/framework/listframe/SearchBar";
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
 * Renders the filter card for the Fund workspace with header, search bar, and primary action.
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
    <PageFilterFrame
      title="Fund Workspace"
      actions={
        <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap">
          <Button
            variant="outlined"
            disabled={!hasSearch}
            onClick={() => {
              updateParams((params) => {
                params.delete(searchParamName);
              });
            }}
          >
            Reset Filters
          </Button>
          {!canWrite ? null : (
            <Button variant="contained" href={addFundHref}>
              {isInOnboardingMode ? "Onboard Fund" : "Create Fund"}
            </Button>
          )}
        </Stack>
      }
    >
      <SearchBar searchParamName={searchParamName} />
    </PageFilterFrame>
  );
};

export default FundWorkspaceFilter;
