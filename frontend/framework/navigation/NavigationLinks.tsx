"use client";

import { Box, Divider, List } from "@mui/material";
import { type JSX, useEffect, useMemo, useState } from "react";
import NavigationLinkGroup from "./NavigationLinkGroup";
import NavigationLinkItem from "./NavigationLinkItem";
import matchesPath from "./matchesPath";
import navigationItems from "./navigationItems";
import { usePathname } from "next/navigation";

/**
 * Props for the NavigationLinks component.
 */
interface NavigationLinksProps {
  readonly isAdministrator: boolean;
  readonly onNavigate?: (() => void) | undefined;
}

/**
 * Displays the navigation links for the application.
 */
const NavigationLinks = function ({
  isAdministrator,
  onNavigate,
}: NavigationLinksProps): JSX.Element {
  const links = useMemo(
    () => navigationItems(isAdministrator),
    [isAdministrator],
  );
  const pathname = usePathname();
  const [expandedLinkName, setExpandedLinkName] = useState<string | null>(null);

  useEffect(() => {
    const expandedLink = links.find(
      (link) =>
        link.childLinks !== undefined &&
        (matchesPath(pathname, link.href) ||
          link.childLinks.some((child) => matchesPath(pathname, child.href))),
    );
    setExpandedLinkName(expandedLink?.name ?? null);
  }, [links, pathname]);

  return (
    <Box sx={{ overflow: "auto" }}>
      <Divider />
      <List>
        {links.map((link) => {
          if (link.childLinks === undefined) {
            return (
              <NavigationLinkItem
                key={link.name}
                link={link}
                pathname={pathname}
                onNavigate={onNavigate}
              />
            );
          }

          const isExpanded = expandedLinkName === link.name;
          return (
            <NavigationLinkGroup
              key={link.name}
              link={{ ...link, childLinks: link.childLinks }}
              pathname={pathname}
              isExpanded={isExpanded}
              onToggle={() => {
                setExpandedLinkName(isExpanded ? null : link.name);
              }}
              onNavigate={onNavigate}
            />
          );
        })}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationLinks;
