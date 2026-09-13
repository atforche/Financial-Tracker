"use client";

import { Box, Tab, Tabs } from "@mui/material";
import { type JSX, type ReactNode, useId } from "react";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface UrlTab {
  readonly value: string;
  readonly label: string;
  readonly content: ReactNode;
}

interface UrlTabsProps {
  readonly label: string;
  readonly paramName: string;
  readonly tabs: readonly [UrlTab, ...UrlTab[]];
}

/** Shows one view at a time and keeps the selected view in the URL. */
const UrlTabs = function ({
  label,
  paramName,
  tabs,
}: UrlTabsProps): JSX.Element {
  const id = useId();
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const requestedTab = searchParams.get(paramName);
  const activeTab = tabs.find((tab) => tab.value === requestedTab) ?? tabs[0];

  return (
    <Box sx={{ width: "100%", minWidth: 0 }}>
      <Tabs
        value={activeTab.value}
        onChange={(_, value: string) => {
          updateParams((params) => {
            if (value === tabs[0].value) {
              params.delete(paramName);
            } else {
              params.set(paramName, value);
            }
          });
        }}
        aria-label={label}
        variant="scrollable"
        scrollButtons="auto"
        sx={{ mb: 2 }}
      >
        {tabs.map((tab) => (
          <Tab
            key={tab.value}
            value={tab.value}
            label={tab.label}
            id={`${id}-${tab.value}-tab`}
            aria-controls={`${id}-${tab.value}-panel`}
          />
        ))}
      </Tabs>
      <Box
        role="tabpanel"
        id={`${id}-${activeTab.value}-panel`}
        aria-labelledby={`${id}-${activeTab.value}-tab`}
      >
        {activeTab.content}
      </Box>
    </Box>
  );
};

export default UrlTabs;
