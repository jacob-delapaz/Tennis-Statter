<template>
  <div class="page">
    <!-- Match Winner Popup -->
    <div v-if="matchWinner" class="popup-overlay">
      <div class="popup-box">
        <h2>Match Over!</h2>
        <p><strong>{{ matchWinner }}</strong> wins the match!</p>
        <button class="home-btn" @click="goHome">Back to Home</button>
      </div>
    </div>

    <!-- Point Confirmation Popup -->
    <div v-if="showConfirmation" class="popup-overlay">
      <div class="popup-box">
        <p>Confirming that <strong>{{ confirmationWinnerName }}</strong> is the point winner?</p>
        <div class="popup-buttons">
          <button class="confirm-btn yes-btn" @click="confirmPoint">Yes</button>
          <button class="confirm-btn no-btn" @click="cancelConfirmation">No</button>
        </div>
      </div>
    </div>

    <!-- Point Winner Error Popup -->
    <div v-if="showPointWinnerError" class="popup-overlay">
      <div class="popup-box">
        <p>Point Winner Not Selected</p>
        <div class="popup-buttons">
          <button class="confirm-btn yes-btn" @click="dismissPointWinnerError">OK</button>
        </div>
      </div>
    </div>

    <!-- Score Box (top-left) -->
    <div class="score-box">
      <div class="score-row">
        <div class="player-cell">
          <span v-if="server === topPlayer" class="dot">●</span>
          {{ topPlayer }}
        </div>
        <div class="score-cell">{{ inTiebreak ? topTiebreakPoints : pointScores[topPoint] }}</div>
        <div class="score-cell">{{ getSetScore('top', 0) }}</div>
        <div class="score-cell">{{ getSetScore('top', 1) }}</div>
        <div class="score-cell">{{ getSetScore('top', 2) }}</div>
      </div>
      <div class="score-row">
        <div class="player-cell">
          {{ bottomPlayer }}
          <span v-if="server === bottomPlayer" class="dot">●</span>
        </div>
        <div class="score-cell">{{ inTiebreak ? bottomTiebreakPoints : pointScores[bottomPoint] }}</div>
        <div class="score-cell">{{ getSetScore('bottom', 0) }}</div>
        <div class="score-cell">{{ getSetScore('bottom', 1) }}</div>
        <div class="score-cell">{{ getSetScore('bottom', 2) }}</div>
      </div>
    </div>

    <!-- Point Trail -->
    <div class="point-trail-box" ref="pointTrailBox">
      <table class="point-trail-table">
        <thead>
          <tr>
            <th>Point</th>
            <th>Winner</th>
            <th>1st Serve</th>
            <th>2nd Serve</th>
            <th>Return</th>
            <th>Point End</th>
            <th>Strategy</th>
            <th>S&V</th>
          </tr>
        </thead>
        <tbody>
          <!-- Empty placeholder rows at top to ensure minimum 3 rows -->
          <tr v-for="n in Math.max(0, 3 - pointHistory.length)" :key="'empty-' + n" class="empty-row">
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
          </tr>
          <tr v-for="(point, index) in pointHistory" :key="index">
            <td>{{ point.setNumber }}-{{ point.gameNumber }}-{{ point.pointNumber }}</td>
            <td>{{ point.pointWinner }}</td>
            <td>{{ point.firstServe }}</td>
            <td>{{ point.secondServe }}</td>
            <td>{{ point.returnStats }}</td>
            <td>{{ point.pointEnd }}</td>
            <td>{{ point.strategy }}</td>
            <td>{{ point.serveVolley }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Stat Entry Row 1 -->
    <div class="stat-row">
      <!-- 1st Serve: 2 boxes -->
      <div class="stat-group">
        <div class="stat-label-center">1st Serve</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'firstServeLocation' }" @click="activeCategory = 'firstServeLocation'">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveLocationOptions" :key="opt" 
                   class="stat-option"
                   :class="{ 'selected-row': selections.firstServeLocation === idx }"
                   @click.stop="selections.firstServeLocation = idx; activeCategory = 'firstServeLocation'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'firstServeResult' }" @click="activeCategory = 'firstServeResult'">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.firstServeResult === idx }"
                   @click.stop="selections.firstServeResult = idx; activeCategory = 'firstServeResult'">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- 2nd Serve: 2 boxes -->
      <div class="stat-group" :class="{ 'disabled-group': secondServeDisabled }">
        <div class="stat-label-center">2nd Serve</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'secondServeLocation' && !secondServeDisabled, 'disabled-box': secondServeDisabled }" @click="!secondServeDisabled && (activeCategory = 'secondServeLocation')">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveLocationOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.secondServeLocation === idx && !secondServeDisabled }"
                   @click.stop="!secondServeDisabled && (selections.secondServeLocation = idx, activeCategory = 'secondServeLocation')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'secondServeResult' && !secondServeDisabled, 'disabled-box': secondServeDisabled }" @click="!secondServeDisabled && (activeCategory = 'secondServeResult')">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.secondServeResult === idx && !secondServeDisabled }"
                   @click.stop="!secondServeDisabled && (selections.secondServeResult = idx, activeCategory = 'secondServeResult')">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Return: 3 boxes -->
      <div class="stat-group" :class="{ 'disabled-group': returnDisabled }">
        <div class="stat-label-center">Return</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnSide' && !returnDisabled, 'disabled-box': returnDisabled }" @click="!returnDisabled && (activeCategory = 'returnSide')">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnSideOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnSide === idx && !returnDisabled }"
                   @click.stop="!returnDisabled && (selections.returnSide = idx, activeCategory = 'returnSide')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnType' && !returnDisabled, 'disabled-box': returnDisabled }" @click="!returnDisabled && (activeCategory = 'returnType')">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnTypeOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnType === idx && !returnDisabled }"
                   @click.stop="!returnDisabled && (selections.returnType = idx, activeCategory = 'returnType')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnResult' && !returnDisabled, 'disabled-box': returnDisabled }" @click="!returnDisabled && (activeCategory = 'returnResult')">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnResult === idx && !returnDisabled }"
                   @click.stop="!returnDisabled && (selections.returnResult = idx, activeCategory = 'returnResult')">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Point Winner: 1 box -->
      <div class="stat-group">
        <div class="stat-label-center">Point Winner</div>
        <div class="stat-box point-winner-box selectable-box" :class="{ 'active-box': activeCategory === 'pointWinner' }" @click="activeCategory = 'pointWinner'">
          <div v-for="(option, idx) in pointWinnerOptions" :key="option" 
               class="point-winner-option"
               :class="{ 'selected-row': idx === selections.pointWinner }"
               @click.stop="selections.pointWinner = idx; activeCategory = 'pointWinner'">
            {{ option }}
          </div>
        </div>
      </div>
    </div>

    <!-- Stat Entry Row 2 -->
    <div class="stat-row">
      <!-- Point End: 2 boxes -->
      <div class="stat-group" :class="{ 'disabled-group': pointEndDisabled }">
        <div class="stat-label-center">Point End</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndSide' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'pointEndSide')">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndSideOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndSide === idx && !pointEndDisabled }"
                   @click.stop="!pointEndDisabled && (selections.pointEndSide = idx, activeCategory = 'pointEndSide')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndType' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'pointEndType')">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndTypeOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndType === idx && !pointEndDisabled }"
                   @click.stop="!pointEndDisabled && (selections.pointEndType = idx, activeCategory = 'pointEndType')">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndResult' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'pointEndResult')">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndResult === idx && !pointEndDisabled }"
                   @click.stop="!pointEndDisabled && (selections.pointEndResult = idx, activeCategory = 'pointEndResult')">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Strategy: 1 box -->
      <div class="stat-group" :class="{ 'disabled-group': pointEndDisabled }">
        <div class="stat-label-center">Strategy</div>
        <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'strategy' && !pointEndDisabled, 'disabled-box': pointEndDisabled }" @click="!pointEndDisabled && (activeCategory = 'strategy')">
          <div class="stat-col">
            <div v-for="(opt, idx) in strategyOptions" :key="opt"
                 class="stat-option"
                 :class="{ 'selected-row': selections.strategy === idx && !pointEndDisabled }"
                 @click.stop="!pointEndDisabled && (selections.strategy = idx, activeCategory = 'strategy')">
              {{ opt }}
            </div>
          </div>
        </div>
      </div>
      <!-- Serve & Volley: 1 box -->
      <div class="stat-group" :class="{ 'disabled-group': serveVolleyDisabled }">
        <div class="stat-label-center">S&amp;V</div>
        <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'serveVolley' && !serveVolleyDisabled, 'disabled-box': serveVolleyDisabled }" @click="!serveVolleyDisabled && (activeCategory = 'serveVolley')">
          <div class="stat-col">
            <div v-for="(opt, idx) in serveVolleyOptions" :key="idx"
                 class="stat-option"
                 :class="{ 'selected-row': selections.serveVolley === idx && !serveVolleyDisabled }"
                 @click.stop="!serveVolleyDisabled && (selections.serveVolley = idx, activeCategory = 'serveVolley')">
              {{ opt || '\u00A0' }}
            </div>
          </div>
        </div>
      </div>
      <!-- Summary Stats: 1 box -->
      <div class="stat-group">
        <div class="stat-label-center">&nbsp;</div>
        <div class="stat-box stat-box-summary">
          <div>Summary Stats So Far</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router';
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue';

const route = useRoute();
const router = useRouter();
const topPlayer = route.query.topPlayer as string || 'Top Player';
const bottomPlayer = route.query.bottomPlayer as string || 'Bottom Player';
const initialServer = route.query.server as string || '';

// Current server (alternates each game)
const server = ref(initialServer);

const matchWinner = ref<string | null>(null);

// Ref for point trail box to enable auto-scroll
const pointTrailBox = ref<HTMLElement | null>(null);

const pointScores = ['0', '15', '30', '40', 'Ad'];
const topPoint = ref(0);
const bottomPoint = ref(0);
const topGames = ref(0);
const bottomGames = ref(0);

// Sets: array of [topGamesWon, bottomGamesWon] for each completed set
const topSets = ref<number[]>([]);
const bottomSets = ref<number[]>([]);
const currentSet = ref(1);
const inTiebreak = ref(false);
const topTiebreakPoints = ref(0);
const bottomTiebreakPoints = ref(0);
const tiebreakFirstServer = ref<string | null>(null);

// Point history for the trail
interface PointRecord {
  setNumber: number;
  gameNumber: number;
  pointNumber: number;
  pointWinner: string;
  firstServe: string;
  secondServe: string;
  returnStats: string;
  pointEnd: string;
  strategy: string;
  serveVolley: string;
}
const pointHistory = ref<PointRecord[]>([]);
const currentPointInGame = ref(1);
const currentGameInSet = ref(1);

// Stat categories and their options
const statCategories = [
  'firstServeLocation',
  'firstServeResult',
  'secondServeLocation', 
  'secondServeResult',
  'returnSide',
  'returnType',
  'returnResult',
  'pointWinner',
  'pointEndSide',
  'pointEndType',
  'pointEndResult',
  'strategy',
  'serveVolley'
] as const;

type StatCategory = typeof statCategories[number];

// Options for each category
const serveLocationOptions = ['Wide', 'Body', 'Center', 'In Net'];
const serveResultOptions = ['In Play', 'Ace', 'Winner', 'Fault'];
const returnSideOptions = ['Forehand', 'Backhand'];
const returnTypeOptions = ['Ground', 'Pass', 'Approach', 'Lob', 'Drop'];
const returnResultOptions = ['In Play', 'Winner', 'Unforced', 'Forced'];
const pointWinnerOptions = [topPlayer, bottomPlayer];
const pointEndSideOptions = ['Forehand', 'Backhand'];
const pointEndTypeOptions = ['Ground', 'Pass', 'Approach', 'Lob', 'Drop', 'Overhead', 'Volley'];
const pointEndResultOptions = ['Winner', 'Unforced', 'Forced'];
const strategyOptions = ['Baseline', `${topPlayer} At Net`, `${bottomPlayer} At Net`, 'Both At Net'];
const serveVolleyOptions = ['', 'Serve & Volley'];

// Confirmation popup state
const showConfirmation = ref(false);

// Error popup state for point winner not selected
const showPointWinnerError = ref(false);

// 2nd serve is disabled if 1st serve result is "In Play" (index 0), "Ace" (1), or "Winner" (2)
// Only "Fault" (index 3) allows a 2nd serve
const secondServeDisabled = computed(() => {
  const firstResult = selections.value.firstServeResult;
  return firstResult !== null && firstResult !== 3; // Not fault = no 2nd serve
});

// Return is disabled if:
// - Serve result is "Ace" (1) or "Winner" (2) - server wins point
// - 2nd serve result is "Fault" (3) - double fault, returner wins point
const returnDisabled = computed(() => {
  if (secondServeDisabled.value) {
    // No 2nd serve, check 1st serve result
    const firstResult = selections.value.firstServeResult;
    return firstResult === 1 || firstResult === 2; // Ace or Winner
  } else {
    // 2nd serve happened, check 2nd serve result
    const secondResult = selections.value.secondServeResult;
    // Ace, Winner, or Fault (double fault) all skip return
    return secondResult === 1 || secondResult === 2 || secondResult === 3;
  }
});

// Get server index for point winner (0 = top player, 1 = bottom player)
const serverIndex = computed(() => {
  return server.value === topPlayer ? 0 : 1;
});

// Get returner index (opposite of server)
const returnerIndex = computed(() => {
  return server.value === topPlayer ? 1 : 0;
});

// Point End is disabled if return result is "Winner" (1), "Unforced" (2), or "Forced" (3)
// Also disabled if return itself is disabled (ace/winner/double fault)
const pointEndDisabled = computed(() => {
  if (returnDisabled.value) {
    return true;
  }
  const returnResult = selections.value.returnResult;
  // Winner (1), Unforced (2), or Forced (3) all end the point on return
  return returnResult === 1 || returnResult === 2 || returnResult === 3;
});

// Serve & Volley is disabled when:
// - Double fault (2nd serve result is Fault)
// - Serve is Ace or Winner (1st or 2nd serve)
const serveVolleyDisabled = computed(() => {
  const firstResult = selections.value.firstServeResult;
  const secondResult = selections.value.secondServeResult;
  
  // If 1st serve was Ace (1) or Winner (2), disable
  if (firstResult === 1 || firstResult === 2) {
    return true;
  }
  
  // If 2nd serve happened (1st was fault)
  if (!secondServeDisabled.value) {
    // Double fault (Fault = 3) or Ace (1) or Winner (2)
    if (secondResult === 1 || secondResult === 2 || secondResult === 3) {
      return true;
    }
  }
  
  return false;
});

// Get the name of the point winner for confirmation popup
const confirmationWinnerName = computed(() => {
  const winnerIndex = selections.value.pointWinner ?? 0;
  return pointWinnerOptions[winnerIndex];
});

// Current active category (for keyboard navigation)
const activeCategory = ref<StatCategory>('firstServeLocation');

// Selection state for each category (null = not yet visited/selected)
const selections = ref<Record<StatCategory, number | null>>({
  firstServeLocation: 0,  // Default to Wide
  firstServeResult: null,
  secondServeLocation: null,
  secondServeResult: null,
  returnSide: null,
  returnType: null,
  returnResult: null,
  pointWinner: null,
  pointEndSide: null,
  pointEndType: null,
  pointEndResult: null,
  strategy: null,
  serveVolley: null
});

// Get options array for a category
function getOptionsForCategory(category: StatCategory): string[] {
  switch (category) {
    case 'firstServeLocation':
    case 'secondServeLocation':
      return serveLocationOptions;
    case 'firstServeResult':
    case 'secondServeResult':
      return serveResultOptions;
    case 'returnSide':
      return returnSideOptions;
    case 'returnType':
      return returnTypeOptions;
    case 'returnResult':
      return returnResultOptions;
    case 'pointWinner':
      return pointWinnerOptions;
    case 'pointEndSide':
      return pointEndSideOptions;
    case 'pointEndType':
      return pointEndTypeOptions;
    case 'pointEndResult':
      return pointEndResultOptions;
    case 'strategy':
      return strategyOptions;
    case 'serveVolley':
      return serveVolleyOptions;
  }
}

// Helper to get the score for a set column
// Column 0 = first set, column 1 = second set, etc.
// Shows completed set score OR live games if it's the current set
function getSetScore(player: 'top' | 'bottom', colIndex: number): string | number {
  const sets = player === 'top' ? topSets.value : bottomSets.value;
  const games = player === 'top' ? topGames.value : bottomGames.value;
  
  // If this column has a completed set, show it
  if (colIndex < sets.length) {
    return sets[colIndex] ?? '';
  }
  // If match is over, don't show live games for unplayed sets
  if (matchWinner.value) {
    return '';
  }
  // If this is the current set column (immediately after completed sets), show live games
  if (colIndex === sets.length) {
    return games;
  }
  // Future sets - empty
  return '';
}

// Auto-select "Serve & Volley" if return type is Pass (1) or Lob (3)
function autoSelectServeVolleyIfNeeded() {
  const returnType = selections.value.returnType;
  if (returnType === 1 || returnType === 3) {
    // Pass or Lob - auto-select Serve & Volley (index 1)
    selections.value.serveVolley = 1;
  }
}

// Auto-select strategy based on point end type, result, and winner
// pointEndType: 1 = 'Pass', 2 = 'Approach', 3 = 'Lob', 5 = 'Overhead', 6 = 'Volley'
// pointEndResult: 0 = 'Winner', 1 = 'Unforced', 2 = 'Forced'
// strategy: 1 = topPlayer At Net, 2 = bottomPlayer At Net
function autoSelectStrategyIfNeeded() {
  // Use 0 as default for null (unvisited) values
  const pointEndType = selections.value.pointEndType ?? 0;
  const pointEndResult = selections.value.pointEndResult ?? 0;
  const pointWinner = selections.value.pointWinner ?? 0;
  
  // Pass (1) or Lob (3): opponent was at net
  const isPassOrLob = pointEndType === 1 || pointEndType === 3;
  // Approach (2), Overhead (5), Volley (6): player hitting was at net
  const isNetShot = pointEndType === 2 || pointEndType === 5 || pointEndType === 6;
  
  if (!isPassOrLob && !isNetShot) return;
  
  if (isPassOrLob) {
    // Pass/Lob logic: opponent was at net
    if (pointEndResult === 0) {
      // Winner - the player at net lost the point (got passed/lobbed)
      if (pointWinner === 0) {
        // Top player won with a passing/lob winner → Bottom was at net
        selections.value.strategy = 2; // bottomPlayer At Net
      } else if (pointWinner === 1) {
        // Bottom player won with a passing/lob winner → Top was at net
        selections.value.strategy = 1; // topPlayer At Net
      }
    } else if (pointEndResult === 1 || pointEndResult === 2) {
      // Unforced or Forced error - the player who made the error was passing/lobbing, opponent was at net
      if (pointWinner === 0) {
        // Top player won → Bottom made passing/lob error → Top was at net
        selections.value.strategy = 1; // topPlayer At Net
      } else if (pointWinner === 1) {
        // Bottom player won → Top made passing/lob error → Bottom was at net
        selections.value.strategy = 2; // bottomPlayer At Net
      }
    }
  } else if (isNetShot) {
    // Approach/Overhead/Volley logic: player hitting was at net
    if (pointEndResult === 0) {
      // Winner - the player at net won with an approach/overhead/volley winner
      if (pointWinner === 0) {
        // Top player won with net shot winner → Top was at net
        selections.value.strategy = 1; // topPlayer At Net
      } else if (pointWinner === 1) {
        // Bottom player won with net shot winner → Bottom was at net
        selections.value.strategy = 2; // bottomPlayer At Net
      }
    } else if (pointEndResult === 1 || pointEndResult === 2) {
      // Unforced or Forced error - the player attempting the net shot made an error
      if (pointWinner === 0) {
        // Top player won → Bottom made net shot error → Bottom was at net
        selections.value.strategy = 2; // bottomPlayer At Net
      } else if (pointWinner === 1) {
        // Bottom player won → Top made net shot error → Top was at net
        selections.value.strategy = 1; // topPlayer At Net
      }
    }
  }
}

function handleKey(e: KeyboardEvent) {
  // Don't allow input after match is over
  if (matchWinner.value) {
    return;
  }
  
  // Handle error popup separately
  if (showPointWinnerError.value) {
    if (e.key === 'Enter' || e.key === 'Escape') {
      dismissPointWinnerError();
    }
    // Block all other keys when error popup is showing
    return;
  }
  
  // Handle confirmation popup separately
  if (showConfirmation.value) {
    if (e.key === 'Enter') {
      confirmPoint();
    } else if (e.key === 'Escape') {
      cancelConfirmation();
    }
    // Block all other keys when confirmation is showing
    return;
  }
  
  const currentOptions = getOptionsForCategory(activeCategory.value);
  const currentIndex = statCategories.indexOf(activeCategory.value);
  const currentSelection = selections.value[activeCategory.value];
  
  if (e.key === 'ArrowDown') {
    e.preventDefault();
    // If null, start at top (0)
    // If at bottom, clear selection (null)
    // Otherwise, move down
    if (currentSelection === null) {
      selections.value[activeCategory.value] = 0;
    } else if (currentSelection === currentOptions.length - 1) {
      // At bottom option, clear selection
      selections.value[activeCategory.value] = null;
    } else {
      selections.value[activeCategory.value] = currentSelection + 1;
    }
  } else if (e.key === 'ArrowUp') {
    e.preventDefault();
    // If null, go to bottom option
    // If at top (0), clear selection (null)
    // Otherwise, move up
    if (currentSelection === null) {
      selections.value[activeCategory.value] = currentOptions.length - 1;
    } else if (currentSelection === 0) {
      // At top option, clear selection
      selections.value[activeCategory.value] = null;
    } else {
      selections.value[activeCategory.value] = currentSelection - 1;
    }
  } else if (e.key === 'ArrowRight') {
    e.preventDefault();
    // Check if leaving a serve location box with "In Net" selected
    // If so, auto-set result to "Fault" and skip to next box after result
    if (activeCategory.value === 'firstServeLocation' && selections.value.firstServeLocation === 3) {
      // "In Net" selected - auto-set result to "Fault" (index 3) and skip to secondServeLocation
      selections.value.firstServeResult = 3;
      activeCategory.value = 'secondServeLocation';
      if (selections.value.secondServeLocation === null) {
        selections.value.secondServeLocation = 0;
      }
    } else if (activeCategory.value === 'firstServeResult' && secondServeDisabled.value) {
      // 1st serve was in play/ace/winner - skip 2nd serve
      // If ace or winner, also skip return and auto-select server as point winner
      const firstResult = selections.value.firstServeResult;
      if (firstResult === 1 || firstResult === 2) {
        // Ace or Winner - skip to pointWinner, auto-select server
        selections.value.pointWinner = serverIndex.value;
        activeCategory.value = 'pointWinner';
      } else {
        // In Play - go to returnSide
        activeCategory.value = 'returnSide';
        if (selections.value.returnSide === null) {
          selections.value.returnSide = 0;
        }
      }
    } else if (activeCategory.value === 'secondServeLocation' && selections.value.secondServeLocation === 3) {
      // "In Net" selected - auto-set result to "Fault" (double fault)
      // Skip return, auto-select returner as point winner
      selections.value.secondServeResult = 3;
      selections.value.pointWinner = returnerIndex.value;
      activeCategory.value = 'pointWinner';
    } else if (activeCategory.value === 'secondServeResult') {
      // Check 2nd serve result
      const secondResult = selections.value.secondServeResult;
      if (secondResult === 1 || secondResult === 2) {
        // Ace or Winner - skip return, auto-select server
        selections.value.pointWinner = serverIndex.value;
        activeCategory.value = 'pointWinner';
      } else if (secondResult === 3) {
        // Fault (double fault) - skip return, auto-select returner
        selections.value.pointWinner = returnerIndex.value;
        activeCategory.value = 'pointWinner';
      } else {
        // In Play - normal navigation to returnSide
        activeCategory.value = 'returnSide';
        if (selections.value.returnSide === null) {
          selections.value.returnSide = 0;
        }
      }
    } else if (activeCategory.value === 'returnType') {
      // Auto-select Serve & Volley if return type is Pass or Lob when leaving this box
      autoSelectServeVolleyIfNeeded();
      // Normal navigation to returnResult
      activeCategory.value = 'returnResult';
      if (selections.value.returnResult === null) {
        selections.value.returnResult = 0;
      }
    } else if (activeCategory.value === 'returnResult') {
      // Check if return result is Winner - skip point end, auto-select returner
      const returnResult = selections.value.returnResult;
      if (returnResult === 1) {
        // Winner - skip point end, auto-select returner
        selections.value.pointWinner = returnerIndex.value;
        activeCategory.value = 'pointWinner';
      } else {
        // In Play, Unforced, or Forced - normal navigation to pointWinner
        const nextIndex = (currentIndex + 1) % statCategories.length;
        const nextCategory = statCategories[nextIndex]!;
        activeCategory.value = nextCategory;
        if (selections.value[nextCategory] === null) {
          selections.value[nextCategory] = 0;
        }
      }
    } else if (activeCategory.value === 'strategy') {
      // If moving from strategy to serveVolley but serveVolley is disabled, wrap to firstServeLocation
      if (serveVolleyDisabled.value) {
        activeCategory.value = 'firstServeLocation';
        selections.value.firstServeLocation = 0;
      } else {
        activeCategory.value = 'serveVolley';
        if (selections.value.serveVolley === null) {
          selections.value.serveVolley = 0;
        }
      }
    } else if (activeCategory.value === 'pointEndResult') {
      // Auto-select strategy based on point end type (Pass) and result when exiting pointEndResult
      autoSelectStrategyIfNeeded();
      // Normal navigation to strategy
      activeCategory.value = 'strategy';
      if (selections.value.strategy === null) {
        selections.value.strategy = 0;
      }
    } else {
      // Normal navigation - move to next category
      const nextIndex = (currentIndex + 1) % statCategories.length;
      const nextCategory = statCategories[nextIndex]!;
      activeCategory.value = nextCategory;
      // If entering a box for the first time, set selection to 0 (first option)
      if (selections.value[nextCategory] === null) {
        selections.value[nextCategory] = 0;
      }
    }
  } else if (e.key === 'ArrowLeft') {
    e.preventDefault();
    // Auto-select Serve & Volley if leaving return area with Pass or Lob selected
    if (activeCategory.value === 'returnSide' || activeCategory.value === 'returnResult') {
      autoSelectServeVolleyIfNeeded();
    }
    // Auto-select strategy if leaving strategy box (may have edited point end values)
    if (activeCategory.value === 'strategy') {
      autoSelectStrategyIfNeeded();
    }
    // If at pointWinner and point end is disabled due to return winner, skip back to returnResult
    if (activeCategory.value === 'pointWinner' && pointEndDisabled.value && !returnDisabled.value) {
      activeCategory.value = 'returnResult';
    } else if (activeCategory.value === 'pointWinner' && returnDisabled.value) {
      // If return is disabled, skip back to serve result
      if (secondServeDisabled.value) {
        activeCategory.value = 'firstServeResult';
      } else {
        activeCategory.value = 'secondServeResult';
      }
    } else if (activeCategory.value === 'returnSide' && secondServeDisabled.value) {
      // If at returnSide and 2nd serve is disabled, skip back to firstServeResult
      activeCategory.value = 'firstServeResult';
    } else if (activeCategory.value === 'firstServeLocation' && serveVolleyDisabled.value) {
      // If at first box and serveVolley is disabled, skip back to strategy
      activeCategory.value = 'strategy';
    } else {
      // Normal navigation - move to previous category
      const prevIndex = (currentIndex - 1 + statCategories.length) % statCategories.length;
      const prevCategory = statCategories[prevIndex]!;
      activeCategory.value = prevCategory;
      // If entering a box for the first time, set selection to 0 (first option)
      if (selections.value[prevCategory] === null) {
        selections.value[prevCategory] = 0;
      }
    }
  } else if (e.key === 'Enter') {
    // Check if at serve result with Ace or Winner - auto-select server as point winner
    const isFirstServeAceOrWinner = activeCategory.value === 'firstServeResult' && 
      (selections.value.firstServeResult === 1 || selections.value.firstServeResult === 2);
    const isSecondServeAceOrWinner = activeCategory.value === 'secondServeResult' && 
      (selections.value.secondServeResult === 1 || selections.value.secondServeResult === 2);
    
    // Check if at 2nd serve location with "In Net" selected - double fault, returner wins
    const isSecondServeInNet = activeCategory.value === 'secondServeLocation' && 
      selections.value.secondServeLocation === 3;
    
    // Check if at 2nd serve result with "Fault" selected - double fault, returner wins
    const isSecondServeFault = activeCategory.value === 'secondServeResult' && 
      selections.value.secondServeResult === 3;
    
    // Check if at return result with "Winner" selected - returner wins
    const isReturnWinner = activeCategory.value === 'returnResult' && 
      selections.value.returnResult === 1;
    
    // Check if at return result with "Unforced" (2) or "Forced" (3) selected - server wins
    const isReturnError = activeCategory.value === 'returnResult' && 
      (selections.value.returnResult === 2 || selections.value.returnResult === 3);
    
    // Auto-select Serve & Volley if return type is Pass or Lob
    autoSelectServeVolleyIfNeeded();
    
    if (isFirstServeAceOrWinner || isSecondServeAceOrWinner || isReturnError) {
      // Auto-select server as point winner
      selections.value.pointWinner = serverIndex.value;
    } else if (isSecondServeInNet || isSecondServeFault || isReturnWinner) {
      // Returner wins - double fault or return winner
      if (isSecondServeInNet) {
        selections.value.secondServeResult = 3; // Set result to Fault
      }
      selections.value.pointWinner = returnerIndex.value;
    }
    
    // Auto-select strategy based on point end type (Pass) and result
    autoSelectStrategyIfNeeded();
    
    // Validate that point winner is selected
    if (selections.value.pointWinner === null) {
      showPointWinnerError.value = true;
      return;
    }
    
    // Show confirmation popup
    showConfirmation.value = true;
  }
}

function dismissPointWinnerError() {
  showPointWinnerError.value = false;
}

function confirmPoint() {
  // Hide the confirmation popup
  showConfirmation.value = false;
  // Process the point
  processPoint();
}

function cancelConfirmation() {
  // Hide the confirmation popup, keep selections as-is for editing
  showConfirmation.value = false;
}

function processPoint() {
  // Helper to get selection value (default to 0 if null/unvisited)
  const getSel = (cat: StatCategory) => selections.value[cat] ?? 0;
  
  // Format first serve: "Location, Result"
  const firstServe = `${serveLocationOptions[getSel('firstServeLocation')]}, ${serveResultOptions[getSel('firstServeResult')]}`;
  
  // Format second serve: "Location, Result" or empty if not used
  const secondServeUsed = getSel('firstServeResult') === 3; // Fault
  const secondServe = secondServeUsed 
    ? `${serveLocationOptions[getSel('secondServeLocation')]}, ${serveResultOptions[getSel('secondServeResult')]}`
    : '';
  
  // Format return: "Side, Type, Result" or empty if skipped
  const returnSkipped = returnDisabled.value;
  const returnStats = returnSkipped
    ? ''
    : `${returnSideOptions[getSel('returnSide')]}, ${returnTypeOptions[getSel('returnType')]}, ${returnResultOptions[getSel('returnResult')]}`;
  
  // Format point end: "Side, Type, Result" or empty if skipped
  const pointEndSkipped = pointEndDisabled.value;
  const pointEnd = pointEndSkipped
    ? ''
    : `${pointEndSideOptions[getSel('pointEndSide')]}, ${pointEndTypeOptions[getSel('pointEndType')]}, ${pointEndResultOptions[getSel('pointEndResult')]}`;
  
  // Get strategy (empty if point end is disabled/skipped) and serve & volley
  const strategy = pointEndSkipped ? '' : (strategyOptions[getSel('strategy')] ?? '');
  const serveVolley = serveVolleyOptions[getSel('serveVolley')] ?? '';
  
  // Create point record
  const pointRecord: PointRecord = {
    setNumber: currentSet.value,
    gameNumber: currentGameInSet.value,
    pointNumber: currentPointInGame.value,
    pointWinner: pointWinnerOptions[getSel('pointWinner')] ?? '',
    firstServe,
    secondServe,
    returnStats,
    pointEnd,
    strategy,
    serveVolley
  };
  
  // Add to history
  pointHistory.value.push(pointRecord);
  
  // Scroll point trail to bottom to show latest point
  nextTick(() => {
    if (pointTrailBox.value) {
      pointTrailBox.value.scrollTop = pointTrailBox.value.scrollHeight;
    }
  });
  
  // Increment point counter
  currentPointInGame.value++;
  
  console.log('Point recorded:', pointRecord);
  
  // Update score based on point winner (default to top player if not selected)
  const pointWinnerSelection = getSel('pointWinner');
  if (inTiebreak.value) {
    // Tiebreak scoring
    if (pointWinnerSelection === 0) {
      topTiebreakPoints.value++;
    } else {
      bottomTiebreakPoints.value++;
    }
    // Tiebreak serve switching: first serve is 1 point, then alternate every 2 points
    const totalPoints = topTiebreakPoints.value + bottomTiebreakPoints.value;
    if (totalPoints === 1 || (totalPoints > 1 && (totalPoints - 1) % 2 === 0)) {
      switchServer();
    }
    checkTiebreakWin();
  } else {
    // Regular game scoring
    if (pointWinnerSelection === 0) {
      updateScore(topPoint, bottomPoint, topGames, bottomGames);
    } else {
      updateScore(bottomPoint, topPoint, bottomGames, topGames);
    }
  }
  
  // Reset selections to defaults for next point (except point winner stays at 0)
  resetSelectionsForNextPoint();
}

function resetSelectionsForNextPoint() {
  // Reset all selections to null (unvisited) except first box
  selections.value.firstServeLocation = 0;  // First box starts selected
  selections.value.firstServeResult = null;
  selections.value.secondServeLocation = null;
  selections.value.secondServeResult = null;
  selections.value.returnSide = null;
  selections.value.returnType = null;
  selections.value.returnResult = null;
  selections.value.pointWinner = null;
  selections.value.pointEndSide = null;
  selections.value.pointEndType = null;
  selections.value.pointEndResult = null;
  selections.value.strategy = null;
  selections.value.serveVolley = null;
  // Reset active category to first one
  activeCategory.value = 'firstServeLocation';
}

function updateScore(
  myPoint: { value: number },
  oppPoint: { value: number },
  myGames: { value: number },
  oppGames: { value: number }
) {
  // Deuce: both at 40 (index 3)
  if (myPoint.value === 3 && oppPoint.value === 3) {
    // Go to advantage
    myPoint.value = 4;
  } else if (myPoint.value === 4) {
    // Win from advantage
    winGame(myGames, oppGames);
    myPoint.value = 0;
    oppPoint.value = 0;
  } else if (oppPoint.value === 4) {
    // Opponent had advantage, back to deuce
    oppPoint.value = 3;
  } else if (myPoint.value === 3 && oppPoint.value < 3) {
    // Win game from 40 when opponent doesn't have 40
    winGame(myGames, oppGames);
    myPoint.value = 0;
    oppPoint.value = 0;
  } else if (myPoint.value < 3) {
    myPoint.value++;
  }
}

function switchServer() {
  server.value = server.value === topPlayer ? bottomPlayer : topPlayer;
}

function winGame(myGames: { value: number }, _oppGames: { value: number }) {
  myGames.value++;
  currentPointInGame.value = 1; // Reset point counter for new game
  currentGameInSet.value++; // Increment game counter
  switchServer(); // Server alternates after each game
  checkSetWin();
}

function checkSetWin() {
  const tg = topGames.value;
  const bg = bottomGames.value;

  // Check for tiebreak at 6-6
  if (tg === 6 && bg === 6) {
    inTiebreak.value = true;
    topTiebreakPoints.value = 0;
    bottomTiebreakPoints.value = 0;
    tiebreakFirstServer.value = server.value; // Remember who serves first in tiebreak
    return;
  }

  // Check if top player wins set
  if (tg >= 6 && tg - bg >= 2) {
    winSet('top');
    return;
  }

  // Check if bottom player wins set
  if (bg >= 6 && bg - tg >= 2) {
    winSet('bottom');
    return;
  }
}

function checkTiebreakWin() {
  const tp = topTiebreakPoints.value;
  const bp = bottomTiebreakPoints.value;

  // First to 7 with 2 point lead
  if (tp >= 7 && tp - bp >= 2) {
    topGames.value++; // Tiebreak counts as 1 game (7-6)
    winSet('top');
  } else if (bp >= 7 && bp - tp >= 2) {
    bottomGames.value++;
    winSet('bottom');
  }
}

function winSet(_winner: 'top' | 'bottom') {
  // Store completed set scores
  topSets.value.push(topGames.value);
  bottomSets.value.push(bottomGames.value);

  // Check if match is won (best of 3 = first to 2 sets)
  const topSetsWon = topSets.value.filter((s, i) => s > (bottomSets.value[i] ?? 0)).length;
  const bottomSetsWon = bottomSets.value.filter((s, i) => s > (topSets.value[i] ?? 0)).length;

  if (topSetsWon >= 2) {
    matchWinner.value = topPlayer;
    return;
  }
  if (bottomSetsWon >= 2) {
    matchWinner.value = bottomPlayer;
    return;
  }

  // Reset for next set
  topGames.value = 0;
  bottomGames.value = 0;
  topPoint.value = 0;
  bottomPoint.value = 0;
  
  // After a tiebreak, the player who served second in the tiebreak serves first in the next set
  if (inTiebreak.value && tiebreakFirstServer.value) {
    server.value = tiebreakFirstServer.value === topPlayer ? bottomPlayer : topPlayer;
  }
  
  inTiebreak.value = false;
  topTiebreakPoints.value = 0;
  bottomTiebreakPoints.value = 0;
  tiebreakFirstServer.value = null;
  currentSet.value++;
  currentGameInSet.value = 1; // Reset game counter for new set
}

function goHome() {
  router.push('/');
}

onMounted(() => {
  window.addEventListener('keydown', handleKey);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKey);
});
</script>

<style scoped>
.page {
  padding: 16px;
  font-family: sans-serif;
}
.page-label {
  font-size: 0.9rem;
  color: #888;
  margin-bottom: 8px;
}

/* Score Box */
.score-box {
  display: inline-block;
  border: 2px solid #111;
  margin-bottom: 24px;
}
.score-row {
  display: flex;
}
.player-cell {
  min-width: 160px;
  padding: 8px 12px;
  border-right: 2px solid #111;
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}
.score-cell {
  width: 40px;
  min-width: 40px;
  padding: 8px 0;
  text-align: center;
  border-right: 2px solid #111;
  font-weight: 500;
}
.score-cell:last-child {
  border-right: none;
}
.score-row:first-child .player-cell,
.score-row:first-child .score-cell {
  border-bottom: 2px solid #111;
}
.dot {
  color: #8e267d;
  font-size: 1.2rem;
}

/* Point Trail */
.point-trail-box {
  border: 2px solid #111;
  max-height: 130px;
  padding: 0;
  margin-bottom: 24px;
  overflow: auto;
}
.point-trail-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
  table-layout: fixed;
}
.point-trail-table th,
.point-trail-table td {
  border: 1px solid #ccc;
  padding: 8px 6px;
  text-align: center;
  vertical-align: middle;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.point-trail-table th {
  background-color: #f5f5f5;
  font-weight: 600;
  position: sticky;
  top: 0;
  z-index: 1;
}
.point-trail-table tbody tr:nth-child(even) {
  background-color: #fafafa;
}
.point-trail-table tbody tr.empty-row {
  background-color: transparent;
}
.point-trail-label {
  color: #aaa;
  font-size: 1rem;
}

/* Stat Rows */
.stat-row {
  display: flex;
  gap: 0;
  margin-bottom: 24px;
  align-items: stretch;
  width: 100%;
}
.stat-group {
  display: flex;
  flex-direction: column;
}
.stat-group:last-child {
  flex: 1;
}
.stat-label {
  font-size: 0.95rem;
  font-weight: 600;
  margin-bottom: 4px;
  padding-left: 4px;
  min-height: 20px;
}
.stat-label-center {
  font-size: 0.95rem;
  font-weight: 600;
  margin-bottom: 4px;
  min-height: 20px;
  text-align: center;
}
.stat-box {
  display: flex;
  border: 2px solid #111;
  flex: 1;
}
.stat-box-single {
  min-width: 100px;
}
.stat-box-summary {
  min-width: 180px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #888;
}
.stat-col {
  padding: 0;
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  gap: 0;
  font-size: 0.95rem;
  text-align: center;
  flex: 1;
}
.stat-col:last-child {
  border-right: none;
}
.stat-box-row {
  display: flex;
  gap: 0;
  flex: 1;
  align-items: stretch;
}
.stat-box-row .stat-box {
  flex: none;
}
.point-winner-box {
  flex-direction: column;
  padding: 0;
}
.point-winner-option {
  padding: 4px 12px;
  font-size: 0.95rem;
  text-align: center;
  cursor: pointer;
}
.stat-option {
  cursor: pointer;
  padding: 4px 12px;
  width: 100%;
  box-sizing: border-box;
}
.stat-option:hover,
.point-winner-option:hover {
  background-color: #e0e0e0;
}
.selectable-box {
  cursor: pointer;
}
.active-box {
  border-color: #8e267d;
  border-width: 3px;
}
.selected-row {
  background-color: #fff59d;
}
.disabled-group {
  opacity: 0.4;
}
.disabled-box {
  background-color: #e0e0e0;
  cursor: not-allowed;
}

/* Match Winner Popup */
.popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.popup-box {
  background: #fff;
  border: 3px solid #111;
  padding: 32px 48px;
  text-align: center;
  min-width: 300px;
}
.popup-box h2 {
  margin: 0 0 16px 0;
  font-size: 1.5rem;
}
.popup-box p {
  margin: 0 0 24px 0;
  font-size: 1.2rem;
}
.home-btn {
  background: #8e267d;
  color: #fff;
  border: none;
  padding: 12px 24px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
}
.home-btn:hover {
  background: #6d1d5f;
}

/* Confirmation popup buttons */
.popup-buttons {
  display: flex;
  gap: 16px;
  justify-content: center;
}
.confirm-btn {
  border: 2px solid #111;
  padding: 10px 32px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
}
.yes-btn {
  background: #8e267d;
  color: #fff;
  border-color: #8e267d;
}
.yes-btn:hover {
  background: #6d1d5f;
  border-color: #6d1d5f;
}
.no-btn {
  background: #fff;
  color: #111;
}
.no-btn:hover {
  background: #f0f0f0;
}
</style>
