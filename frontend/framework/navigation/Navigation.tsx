import { Drawer, type DrawerProps, Toolbar, Typography } from "@mui/material";
import Image from "next/image";
import type { JSX } from "react";
import NavigationLinks from "@/framework/navigation/NavigationLinks";

/**
 * Width of the navigation drawer.
 */
const navigationWidth = 280;

/**
 * Props for the Navigation component.
 */
interface NavigationProps {
  readonly variant?: DrawerProps["variant"];
  readonly open?: boolean;
  readonly onClose?: () => void;
}

/**
 * Displays the navigation using either a permanent or temporary drawer.
 */
const Navigation = function ({
  variant = "permanent",
  open = true,
  onClose,
}: NavigationProps): JSX.Element {
  return (
    <Drawer
      variant={variant}
      open={open}
      onClose={onClose}
      sx={{
        [`& .MuiToolbar-root`]: { padding: "12px" },
        flexShrink: 0,
        width: navigationWidth,
        "& .MuiDrawer-paper": { width: navigationWidth },
      }}
    >
      <Toolbar>
        <Image
          src="/icon.svg"
          height={60}
          width={60}
          alt="Financial Tracker Icon"
        />
        <Typography variant="h6" sx={{ marginLeft: 2 }}>
          Financial Tracker
        </Typography>
      </Toolbar>
      <NavigationLinks
        onNavigate={variant === "temporary" ? onClose : undefined}
      />
    </Drawer>
  );
};

export default Navigation;
export { navigationWidth };
