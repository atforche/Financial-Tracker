import { Drawer, type DrawerProps, Toolbar, Typography } from "@mui/material";
import type { CurrentApplicationUser } from "@/framework/auth/currentApplicationUser";
import CurrentUserMenu from "@/framework/navigation/CurrentUserMenu";
import Image from "next/image";
import type { JSX } from "react";
import NavigationLinks from "@/framework/navigation/NavigationLinks";
import { UserRoleModel } from "@/framework/data/api";

/**
 * Width of the navigation drawer.
 */
const navigationWidth = 280;

/**
 * Props for the Navigation component.
 */
interface NavigationProps {
  readonly applicationUser: CurrentApplicationUser | null;
  readonly variant?: DrawerProps["variant"];
  readonly open?: boolean;
  readonly onClose?: () => void;
  readonly showBranding?: boolean;
  readonly visibility?: "desktop" | "mobile";
  readonly user:
    | {
        readonly name?: string | null;
        readonly email?: string | null;
        readonly image?: string | null;
      }
    | undefined;
}

/**
 * Displays the navigation using either a permanent or temporary drawer.
 */
const Navigation = function ({
  applicationUser,
  variant = "permanent",
  open = true,
  onClose,
  showBranding = true,
  visibility,
  user,
}: NavigationProps): JSX.Element {
  return (
    <Drawer
      variant={variant}
      open={open}
      onClose={onClose}
      ModalProps={variant === "temporary" ? { keepMounted: true } : undefined}
      sx={{
        [`& .MuiToolbar-root`]: { padding: "12px" },
        display:
          visibility === "desktop"
            ? { xs: "none", md: "block" }
            : visibility === "mobile"
              ? { xs: "block", md: "none" }
              : undefined,
        flexShrink: 0,
        width: navigationWidth,
        "& .MuiDrawer-paper": { width: navigationWidth },
      }}
    >
      <Toolbar>
        {showBranding ? (
          <>
            <Image
              src="/icon.svg"
              height={60}
              width={60}
              alt="Financial Tracker Icon"
            />
            <Typography variant="h6" sx={{ marginLeft: 2 }}>
              Financial Tracker
            </Typography>
          </>
        ) : null}
      </Toolbar>
      <NavigationLinks
        isAdministrator={applicationUser?.role === UserRoleModel.Admin}
        onNavigate={variant === "temporary" ? onClose : undefined}
      />
      <CurrentUserMenu applicationUser={applicationUser} user={user} />
    </Drawer>
  );
};

export default Navigation;
export { navigationWidth };
