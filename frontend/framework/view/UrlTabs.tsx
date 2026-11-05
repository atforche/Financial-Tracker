"use client";

import { Box, Tab, Tabs } from "@mui/material";
import { type JSX, type ReactNode, useId } from "react";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * A tab whose selection is stored in the URL.
 */
interface UrlTab {
  readonly value: string;
  readonly label: string;
  readonly content: ReactNode;
}

/**
 * Props for the UrlTabs component.
 */
interface UrlTabsProps {
  readonly label: string;
  readonly paramName: string;
  readonly tabs: readonly [UrlTab, ...UrlTab[]];
}

/**
 * Shows one view at a time behind a segmented control and keeps the selected view in the URL.
 */
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
        sx={{
          width: { xs: "100%", sm: "fit-content" },
          maxWidth: "100%",
          minHeight: 0,
          mb: 2,
          p: 0.5,
          borderRadius: 2,
          bgcolor: "action.hover",
          "& .MuiTabs-indicator": { display: "none" },
          "& .MuiTabs-flexContainer": { gap: 0.5 },
          "& .MuiTab-root": {
            flex: { xs: 1, sm: "none" },
            minHeight: { xs: 44, sm: 34 },
            px: 2,
            py: 0.5,
            borderRadius: 1.5,
            color: "text.secondary",
            fontSize: "0.875rem",
            fontWeight: 500,
            textTransform: "none",
          },
          "& .MuiTab-root:hover": { color: "text.primary" },
          "& .MuiTab-root.Mui-selected": {
            bgcolor: "background.paper",
            boxShadow: 1,
            color: "primary.main",
            fontWeight: 600,
          },
        }}
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
