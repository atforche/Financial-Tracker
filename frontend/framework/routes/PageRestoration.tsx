"use client";

import {
  getCurrentVisit,
  getPageUrl,
  recordArrival,
  saveTrail,
} from "@/framework/routes/visitTrail";
import { useEffect, useLayoutEffect } from "react";
import { usePathname, useSearchParams } from "next/navigation";

// How long to keep reapplying a restored position while the page settles
const restoreDurationMs = 1500;

/**
 * Keeps the user's place on pages they return to. Each visit's scroll position is recorded, and arriving from another
 * page at a page visited earlier in the trail, by a back link, a Cancel button, or a redirect after saving, scrolls
 * back to where that visit was left. Any other arrival starts at the top. Page state kept with usePageState is restored the same way. Mount it before the
 * page content, so each arrival is recorded before the page's own components read their state.
 */
const PageRestoration = function (): null {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const url = getPageUrl(pathname, searchParams.toString());

  useLayoutEffect(() => {
    const target = recordArrival(url).scrollY;
    if (target === null) {
      return undefined;
    }
    /*
     * Owns the scroll position on arrival, the top for a fresh visit or where a return was left, since the router's
     * own scroll can leave a new page partway down. Keeps reapplying it until it sticks, since late content can move it.
     */
    const startedAt = performance.now();
    let frame = 0;
    const stop = (): void => {
      cancelAnimationFrame(frame);
      window.removeEventListener("wheel", stop);
      window.removeEventListener("touchstart", stop);
      window.removeEventListener("keydown", stop);
    };
    const apply = (): void => {
      if (Math.abs(window.scrollY - target) > 1) {
        window.scrollTo(0, target);
      }
      if (performance.now() - startedAt < restoreDurationMs) {
        frame = requestAnimationFrame(apply);
      } else {
        stop();
      }
    };
    window.addEventListener("wheel", stop, { passive: true });
    window.addEventListener("touchstart", stop, { passive: true });
    window.addEventListener("keydown", stop);
    frame = requestAnimationFrame(apply);
    return stop;
  }, [url]);

  useEffect(() => {
    let frame = 0;
    const record = (): void => {
      cancelAnimationFrame(frame);
      frame = requestAnimationFrame(() => {
        const visit = getCurrentVisit();
        if (visit?.url === url) {
          visit.scrollY = window.scrollY;
          saveTrail();
        }
      });
    };
    window.addEventListener("scroll", record, { passive: true });
    return (): void => {
      cancelAnimationFrame(frame);
      window.removeEventListener("scroll", record);
    };
  }, [url]);

  return null;
};

export default PageRestoration;
