"use client";

import {
  type JSX,
  type ReactNode,
  type TransitionStartFunction,
  createContext,
  startTransition,
  use,
  useTransition,
} from "react";
import { Box } from "@mui/material";

/**
 * Starts transitions whose pending state is shown by the shared progress bar.
 */
const NavigationTransitionContext =
  createContext<TransitionStartFunction | null>(null);

/**
 * Props for the NavigationTransitionProvider component.
 */
interface NavigationTransitionProviderProps {
  readonly children: ReactNode;
}

// Every top-level page section except the one holding the page's filters
const pageContentSelector =
  "[data-page-layout] > *:not(:has([data-page-filters]))";

/**
 * Fades the page content, leaving its filters untouched, while a search-param update loads its new page data. The
 * fade is delayed so that quick updates never visibly dim.
 */
const NavigationTransitionProvider = function ({
  children,
}: NavigationTransitionProviderProps): JSX.Element {
  const [isPending, startNavigationTransition] = useTransition();
  return (
    <NavigationTransitionContext value={startNavigationTransition}>
      <Box
        aria-busy={isPending}
        sx={{
          display: "contents",
          [`& ${pageContentSelector}`]: {
            transition: "opacity 150ms ease",
          },
          ...(isPending
            ? {
                [`& ${pageContentSelector}`]: {
                  opacity: 0.5,
                  pointerEvents: "none",
                  transition: "opacity 200ms ease 300ms",
                },
              }
            : {}),
        }}
      >
        {children}
      </Box>
    </NavigationTransitionContext>
  );
};

/**
 * Returns the shared navigation transition, or an untracked transition outside the provider.
 */
const useNavigationTransition = function (): TransitionStartFunction {
  return use(NavigationTransitionContext) ?? startTransition;
};

export { useNavigationTransition };
export default NavigationTransitionProvider;
