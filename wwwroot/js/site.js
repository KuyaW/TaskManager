// Tandem — small, dependency-free interactions.

(function () {
  "use strict";

  // Dismiss the flash message automatically, or on request.
  const flash = document.querySelector("[data-flash]");
  if (flash) {
    const dismiss = () => {
      flash.classList.add("is-leaving");
      flash.addEventListener("animationend", () => flash.remove(), { once: true });
    };

    flash.querySelector("[data-flash-close]")?.addEventListener("click", dismiss);
    window.setTimeout(dismiss, 4500);
  }

  // Filter dropdowns apply immediately; the Filter button remains for no-JS.
  document.querySelectorAll("[data-auto-submit]").forEach((form) => {
    form.querySelectorAll("select").forEach((select) => {
      select.addEventListener("change", () => {
        if (typeof form.requestSubmit === "function") {
          form.requestSubmit();
        } else {
          form.submit();
        }
      });
    });
  });

  // Guard destructive actions with a plain-language confirm.
  document.querySelectorAll("form[data-confirm]").forEach((form) => {
    form.addEventListener("submit", (event) => {
      const message = form.getAttribute("data-confirm");
      if (message && !window.confirm(message)) {
        event.preventDefault();
      }
    });
  });

  // Quick keys: N for a new task, / to search. Ignore when typing.
  const isTyping = (element) =>
    element instanceof HTMLElement &&
    (element.isContentEditable ||
      ["INPUT", "TEXTAREA", "SELECT"].includes(element.tagName));

  document.addEventListener("keydown", (event) => {
    if (event.ctrlKey || event.metaKey || event.altKey || isTyping(event.target)) {
      return;
    }

    if (event.key === "/") {
      const search = document.getElementById("task-search");
      if (search) {
        event.preventDefault();
        search.focus();
        search.select();
      }
    } else if (event.key.toLowerCase() === "n") {
      const newTask = document.getElementById("new-task-link");
      if (newTask) {
        event.preventDefault();
        window.location.assign(newTask.href);
      }
    }
  });
})();
