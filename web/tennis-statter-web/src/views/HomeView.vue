<template>
  <div class="page">
    <h1 class="title">Tennis Statter</h1>

    <button class="big-btn" @click="go">
      Get Started
    </button>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
const router = useRouter();

function go() {
  router.push("/mode");
}
</script>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  gap: 24px;
  background: white;
}
.title {
  font-size: 42px;
  font-weight: 500;
}
.big-btn {
  width: 520px;
  height: 90px;
  border: 3px solid #111;
  background: #f3f0b4;
  font-size: 24px;
  cursor: pointer;
}
</style>
