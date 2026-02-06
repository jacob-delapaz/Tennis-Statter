<template>
  <div class="app">
    <main class="app__main">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
// no imports needed here
</script>

<style>
/* Global baseline */
html, body {
  margin: 0;
  padding: 0;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI",
    Roboto, Oxygen, Ubuntu, Cantarell, "Open Sans", "Helvetica Neue", sans-serif;
}

/* App layout shell */
.app {
  min-height: 100vh;
  background: #fff;
}

.app__main {
  max-width: 900px;
  margin: 0 auto;
  padding: 32px 20px;
}
</style>
