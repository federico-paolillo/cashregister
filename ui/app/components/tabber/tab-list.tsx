import type { KeyboardEvent, ReactNode } from "react";
import { useTabberContext } from "@cashregister/components/tabber/context";

interface TabListProps {
  "aria-label": string;
  children: ReactNode;
}

export function TabList({ "aria-label": ariaLabel, children }: TabListProps) {
  const { setActiveTab } = useTabberContext();

  function onKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    if (
      event.key !== "ArrowLeft" &&
      event.key !== "ArrowRight" &&
      event.key !== "Home" &&
      event.key !== "End"
    ) {
      return;
    }

    if (!(event.target instanceof Element)) {
      return;
    }

    const currentTab = event.target.closest<HTMLButtonElement>(
      "button[role='tab']",
    );
    const tabs = Array.from(
      event.currentTarget.querySelectorAll<HTMLButtonElement>(
        "button[role='tab']",
      ),
    );
    const currentIndex = currentTab ? tabs.indexOf(currentTab) : -1;

    if (currentIndex === -1 || tabs.length === 0) {
      return;
    }

    let nextIndex = currentIndex;

    switch (event.key) {
      case "ArrowLeft":
        nextIndex = currentIndex === 0 ? tabs.length - 1 : currentIndex - 1;
        break;
      case "ArrowRight":
        nextIndex = currentIndex === tabs.length - 1 ? 0 : currentIndex + 1;
        break;
      case "Home":
        nextIndex = 0;
        break;
      case "End":
        nextIndex = tabs.length - 1;
        break;
    }

    const nextTab = tabs[nextIndex];
    const nextTabId = nextTab.dataset.tabId;

    if (nextTabId === undefined) {
      return;
    }

    event.preventDefault();
    nextTab.focus();
    setActiveTab(nextTabId);
  }

  return (
    <div
      aria-label={ariaLabel}
      role="tablist"
      className="tab-list"
      onKeyDown={onKeyDown}
    >
      {children}
    </div>
  );
}
