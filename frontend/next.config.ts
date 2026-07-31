import type { NextConfig } from "next";

const publicOrigin = process.env["PUBLIC_ORIGIN"];
const allowedServerActionOrigins =
  publicOrigin === undefined || publicOrigin.trim() === ""
    ? []
    : [new URL(publicOrigin).host];

const nextConfig: NextConfig = {
  reactCompiler: true,
  typedRoutes: true,
  experimental: {
    serverActions: {
      allowedOrigins: allowedServerActionOrigins,
    },
  },
};

export default nextConfig;
