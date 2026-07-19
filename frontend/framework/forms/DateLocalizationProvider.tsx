"use client";

import type { JSX, ReactNode } from "react";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";

/**
 * Props for the DateLocalizationProvider component.
 */
interface DateLocalizationProviderProps {
  readonly children: ReactNode;
}

/** Provides the shared date adapter configuration for all date controls. */
const DateLocalizationProvider = function ({
  children,
}: DateLocalizationProviderProps): JSX.Element {
  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      {children}
    </LocalizationProvider>
  );
};

export default DateLocalizationProvider;
