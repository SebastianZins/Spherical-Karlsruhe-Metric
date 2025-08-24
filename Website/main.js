// load source data
fetch('./assets/sources.json')
	.then((response) => response.json())
	.then((json) => console.log(json));

// Handle nav item click events
document.querySelectorAll('.nav-link').forEach((item) => {
	item.addEventListener('click', () => {
		document
			.querySelectorAll('.nav-link')
			.forEach((el) => el.classList.remove('active'));
		item.classList.add('active');
	});
});

const sections = [
	'title',
	'voronoi',
	'2d-karlsruhe',
	'spherical-geometry',
	'3d-karlsruhe',
	'showcase',
];

// Handle (de)activating navigation elements on scroll
window.addEventListener('scroll', function () {
	const winScroll = window.scrollY;
	const scrollOffset = window.innerHeight / 2;

	for (let i = 0; i < sections.length; i++) {
		const thisSectionOffsetTop = document.getElementById(
			'section-' + sections[i]
		).offsetTop;
		if (thisSectionOffsetTop < winScroll + scrollOffset) {
			// Remove active class from all nav links
			document.querySelectorAll('.nav-link.active').forEach((link) => {
				link.classList.remove('active');
			});
			// set active flag when section is in view
			const navElement = document.getElementById('nav-' + sections[i]);
			navElement.classList.add('active');
		}
	}
});

const apllyMathJx = (container) => {
	if (window.MathJax) {
		MathJax.typesetPromise([container]).catch((err) =>
			console.error('MathJax error:', err)
		);
	}
};

// load sections
document.addEventListener('DOMContentLoaded', () => {
	sections.forEach((section) => {
		fetch(`sections/${section}/${section}.html`)
			.then((response) => response.text())
			.then((data) => {
				const container = document.getElementById('section-' + section);
				container.innerHTML = data;
				apllyMathJx(container);
			});
	});
});
