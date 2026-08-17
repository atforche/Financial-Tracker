"use client";

import { type JSX, type ReactNode, createContext, useContext } from "react";
import type { Location } from "@/locations/types";

const LocationContext = createContext<readonly Location[]>([]);

/** Provides canonical Locations to transaction entry fields. */
const LocationProvider = function ({
  locations,
  children,
}: {
  readonly locations: readonly Location[];
  readonly children: ReactNode;
}): JSX.Element {
  return (
    <LocationContext.Provider value={locations}>
      {children}
    </LocationContext.Provider>
  );
};

/** Gets canonical Locations available in the current transaction form. */
const useLocations = (): readonly Location[] => useContext(LocationContext);

export { LocationProvider, useLocations };
