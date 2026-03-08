import { Drawer, Toolbar, Typography } from "@mui/material";
import type { JSX } from "react";
import Image from "next/image";
import NavigationLinks from "@/app/NavigationLinks";

const Navigation = function (): JSX.Element {
  const drawerWidth = 260;
  return (
    <Drawer
      variant="permanent"
      sx={{
        [`& .MuiToolbar-root`]: { padding: "12px" },
        flexShrink: 0,
        width: drawerWidth,
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
      <NavigationLinks />
    </Drawer>
  );
};

export default Navigation;
