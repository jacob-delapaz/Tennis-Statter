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

    <div class="page-label">Singles Stat View</div>

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
    <div class="point-trail-box">
      <div class="point-trail-label">Point Trail</div>
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
      <div class="stat-group">
        <div class="stat-label-center">2nd Serve</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'secondServeLocation' }" @click="activeCategory = 'secondServeLocation'">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveLocationOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.secondServeLocation === idx }"
                   @click.stop="selections.secondServeLocation = idx; activeCategory = 'secondServeLocation'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'secondServeResult' }" @click="activeCategory = 'secondServeResult'">
            <div class="stat-col">
              <div v-for="(opt, idx) in serveResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.secondServeResult === idx }"
                   @click.stop="selections.secondServeResult = idx; activeCategory = 'secondServeResult'">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Return: 3 boxes -->
      <div class="stat-group">
        <div class="stat-label-center">Return</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnSide' }" @click="activeCategory = 'returnSide'">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnSideOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnSide === idx }"
                   @click.stop="selections.returnSide = idx; activeCategory = 'returnSide'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnType' }" @click="activeCategory = 'returnType'">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnTypeOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnType === idx }"
                   @click.stop="selections.returnType = idx; activeCategory = 'returnType'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'returnResult' }" @click="activeCategory = 'returnResult'">
            <div class="stat-col">
              <div v-for="(opt, idx) in returnResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.returnResult === idx }"
                   @click.stop="selections.returnResult = idx; activeCategory = 'returnResult'">
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
      <div class="stat-group">
        <div class="stat-label-center">Point End</div>
        <div class="stat-box-row">
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndSide' }" @click="activeCategory = 'pointEndSide'">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndSideOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndSide === idx }"
                   @click.stop="selections.pointEndSide = idx; activeCategory = 'pointEndSide'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndType' }" @click="activeCategory = 'pointEndType'">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndTypeOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndType === idx }"
                   @click.stop="selections.pointEndType = idx; activeCategory = 'pointEndType'">
                {{ opt }}
              </div>
            </div>
          </div>
          <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'pointEndResult' }" @click="activeCategory = 'pointEndResult'">
            <div class="stat-col">
              <div v-for="(opt, idx) in pointEndResultOptions" :key="opt"
                   class="stat-option"
                   :class="{ 'selected-row': selections.pointEndResult === idx }"
                   @click.stop="selections.pointEndResult = idx; activeCategory = 'pointEndResult'">
                {{ opt }}
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Strategy: 1 box -->
      <div class="stat-group">
        <div class="stat-label-center">Strategy</div>
        <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'strategy' }" @click="activeCategory = 'strategy'">
          <div class="stat-col">
            <div v-for="(opt, idx) in strategyOptions" :key="opt"
                 class="stat-option"
                 :class="{ 'selected-row': selections.strategy === idx }"
                 @click.stop="selections.strategy = idx; activeCategory = 'strategy'">
              {{ opt }}
            </div>
          </div>
        </div>
      </div>
      <!-- Serve & Volley: 1 box -->
      <div class="stat-group">
        <div class="stat-label-center">Serve &amp; Volley</div>
        <div class="stat-box selectable-box" :class="{ 'active-box': activeCategory === 'serveVolley' }" @click="activeCategory = 'serveVolley'">
          <div class="stat-col">
            <div v-for="(opt, idx) in serveVolleyOptions" :key="opt"
                 class="stat-option"
                 :class="{ 'selected-row': selections.serveVolley === idx }"
                 @click.stop="selections.serveVolley = idx; activeCategory = 'serveVolley'">
              {{ opt }}
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
import { ref, onMounted, onUnmounted } from 'vue';

const route = useRoute();
const router = useRouter();
const topPlayer = route.query.topPlayer as string || 'Top Player';
const bottomPlayer = route.query.bottomPlayer as string || 'Bottom Player';
const initialServer = route.query.server as string || '';

// Current server (alternates each game)
const server = ref(initialServer);

const matchWinner = ref<string | null>(null);

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
const strategyOptions = ['Baseline', 'Top Player At Net', 'Bottom Player At Net', 'Both At Net'];
const serveVolleyOptions = ['No Serve & Volley', 'Serve & Volley'];

// Current active category (for keyboard navigation)
const activeCategory = ref<StatCategory>('firstServeLocation');

// Selection state for each category (null = not yet visited/selected)
const selections = ref<Record<StatCategory, number | null>>({
  firstServeLocation: 0,  // Start with first box selected
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

function handleKey(e: KeyboardEvent) {
  // Don't allow input after match is over
  if (matchWinner.value) {
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
    // Move to next category
    const nextIndex = (currentIndex + 1) % statCategories.length;
    const nextCategory = statCategories[nextIndex]!;
    activeCategory.value = nextCategory;
    // If entering a box for the first time, set selection to 0 (first option)
    if (selections.value[nextCategory] === null) {
      selections.value[nextCategory] = 0;
    }
  } else if (e.key === 'ArrowLeft') {
    e.preventDefault();
    // Move to previous category
    const prevIndex = (currentIndex - 1 + statCategories.length) % statCategories.length;
    const prevCategory = statCategories[prevIndex]!;
    activeCategory.value = prevCategory;
    // If entering a box for the first time, set selection to 0 (first option)
    if (selections.value[prevCategory] === null) {
      selections.value[prevCategory] = 0;
    }
  } else if (e.key === 'Enter') {
    submitPoint();
  }
}

function submitPoint() {
  // Helper to get selection value (default to 0 if null/unvisited)
  const getSel = (cat: StatCategory) => selections.value[cat] ?? 0;
  
  // Record the point with all current selections
  const pointData = {
    firstServeLocation: serveLocationOptions[getSel('firstServeLocation')],
    firstServeResult: serveResultOptions[getSel('firstServeResult')],
    secondServeLocation: serveLocationOptions[getSel('secondServeLocation')],
    secondServeResult: serveResultOptions[getSel('secondServeResult')],
    returnSide: returnSideOptions[getSel('returnSide')],
    returnType: returnTypeOptions[getSel('returnType')],
    returnResult: returnResultOptions[getSel('returnResult')],
    pointWinner: pointWinnerOptions[getSel('pointWinner')],
    pointEndSide: pointEndSideOptions[getSel('pointEndSide')],
    pointEndType: pointEndTypeOptions[getSel('pointEndType')],
    pointEndResult: pointEndResultOptions[getSel('pointEndResult')],
    strategy: strategyOptions[getSel('strategy')],
    serveVolley: serveVolleyOptions[getSel('serveVolley')]
  };
  
  console.log('Point recorded:', pointData);
  
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
  min-height: 120px;
  padding: 16px;
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
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
  padding: 8px 12px;
  border-right: 2px solid #111;
  display: flex;
  flex-direction: column;
  gap: 2px;
  font-size: 0.95rem;
  text-align: center;
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
  padding: 2px 0;
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
</style>
