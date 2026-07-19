import type { JSX } from "react";
import type { Route } from "next";

/**
 * Represents a child link in the navigation menu, including its name, href, and icon.
 */
interface NavigationChildLink {
  name: string;
  href: Route;
  icon: JSX.Element;
}

/**
 * Represents a navigation link, which may include child links, along with its name, href, and icon.
 */
interface NavigationLink extends NavigationChildLink {
  childLinks?: NavigationChildLink[];
}

export type { NavigationChildLink, NavigationLink };
