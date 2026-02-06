<script setup lang="ts">
import { computed, reactive } from "vue";
import { useRouter } from "vue-router";

type MatchType = "singles" | "doubles";

export type MatchSetup = {
  matchType: MatchType;
  teams: {
    top: string[]; // length 1 or 2
    bottom: string[]; // length 1 or 2
  };
};

const props = defineProps<{
  matchType: MatchType; // passed in from your earlier "Singles/Doubles" selection
}>();

const emit = defineEmits<{
  (e: "submit", setup: MatchSetup): void;
}>();

const router = useRouter();

const form = reactive({
  top1: "",
  top2: "",
  bottom1: "",
  bottom2: "",
});

const requiredCount = computed(() => (props.matchType === "singles" ? 2 : 4));

const allNames = computed(() => {
  const names =
    props.matchType === "singles"
      ? [form.top1, form.bottom1]
      : [form.top1, form.top2, form.bottom1, form.bottom2];

  // trim + remove empty for duplicate checks etc.
  return names.map((n) => n.trim());
});

const hasAllRequired = computed(() => {
  if (props.matchType === "singles") {
    return form.top1.trim().length > 0 && form.bottom1.trim().length > 0;
  }
  return (
    form.top1.trim().length > 0 &&
    form.top2.trim().length > 0 &&
    form.bottom1.trim().length > 0 &&
    form.bottom2.trim().length > 0
  );
});

const hasDuplicates = computed(() => {
  const filled = allNames.value.filter((n) => n.length > 0);
  const set = new Set(filled.map((n) => n.toLowerCase()));
  return set.size !== filled.length;
});

const canSubmit = computed(() => hasAllRequired.value && !hasDuplicates.value);

const validationMessage = computed(() => {
  if (!hasAllRequired.value) return `Enter ${requiredCount.value} names to continue.`;
  if (hasDuplicates.value) return "Names must be unique.";
  return "";
});

function buildSetup(): MatchSetup {
  const top =
    props.matchType === "singles"
      ? [form.top1.trim()]
      : [form.top1.trim(), form.top2.trim()];

  const bottom =
    props.matchType === "singles"
      ? [form.bottom1.trim()]
      : [form.bottom1.trim(), form.bottom2.trim()];

  return {
    matchType: props.matchType,
    teams: { top, bottom },
  };
}

function onSubmit() {
  if (!canSubmit.value) return;
  const setup = buildSetup();
  // Flatten player names for route and encode as comma-separated string
  const players = JSON.stringify([...setup.teams.top, ...setup.teams.bottom]);
  router.push({
    name: "ServeSelection",
    params: {
      matchType: setup.matchType,
    },
    query: {
      players,
    },
  });
}
</script>

<template>
  <div class="page">
    <h1 class="title">Enter the players' names</h1>

    <!-- TOP SIDE -->
    <section class="side">
      <h2 class="sideTitle">Top Side</h2>

      <input
        v-model="form.top1"
        class="input"
        type="text"
        autocomplete="off"
        placeholder="Top Player"
      />

      <input
        v-if="matchType === 'doubles'"
        v-model="form.top2"
        class="input"
        type="text"
        autocomplete="off"
        placeholder="Top Partner"
      />
    </section>

    <!-- BOTTOM SIDE -->
    <section class="side">
      <h2 class="sideTitle">Bottom Side</h2>

      <input
        v-model="form.bottom1"
        class="input"
        type="text"
        autocomplete="off"
        placeholder="Bottom Player"
      />

      <input
        v-if="matchType === 'doubles'"
        v-model="form.bottom2"
        class="input"
        type="text"
        autocomplete="off"
        placeholder="Bottom Partner"
      />
    </section>

    <p v-if="validationMessage" class="error">
      {{ validationMessage }}
    </p>

    <button class="btn" :disabled="!canSubmit" @click="onSubmit">
      Start match
    </button>
  </div>
</template>

<style scoped>
.page {
  max-width: 640px;
  margin: 0 auto;
  padding: 48px 20px;
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.title {
  text-align: center;
  font-size: 32px;
  margin: 0;
}

.side {
  border: 2px solid #111;
  border-radius: 10px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.sideTitle {
  margin: 0 0 6px 0;
  font-size: 18px;
  font-weight: 700;
}

.input {
  height: 52px;
  padding: 0 14px;
  font-size: 18px;
  border: 2px solid #111;
  border-radius: 10px;
}

.error {
  margin: 0;
  text-align: center;
  font-weight: 600;
}

.btn {
  height: 56px;
  font-size: 18px;
  font-weight: 700;
  border: 2px solid #111;
  border-radius: 12px;
  background: white;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
